using CodingHelmet.Optional;
using EnjinSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
/**
 * This script formalizes runtime testing of the SDK into a set of repeatable unit tests.
 */
public class TestSuite : MonoBehaviour
{

    // Constants for preparing the unit tests.
    public bool DEBUG;
    public string PLATFORM_URL;
    public string SERVER_EMAIL;
    public string SERVER_PASSWORD;
    public string TESTING_ITEM_ID;
    public int APP_ID;

    // Runtime-initialized identities.
    EnjinAdminIdentity adminIdentity = null;
    EnjinIdentity testerIdentity = null;

    // Called to continue testing, beginning with creating and fulfilling a trade request for the testing item.
    async Task ContinueTesting(string testerAddress, string developerAddress)
    {
        
        Debug.Log("(6/8) Trading an item from the testing account to the developer account ... ");
        EnjinItemParameter oneTestingItem = EnjinItemParameter.ConstructFungible(TESTING_ITEM_ID, 1);

        // Let the tester request to trade one testing item for one testing item with the developer account.
        IRequestHandler tradeCreationHandler = testerIdentity.CreateTradeItemRequestHandler(new EnjinItemParameter[] { oneTestingItem }, new EnjinItemParameter[] { oneTestingItem }, developerAddress);
        await tradeCreationHandler.RegisterCallback(RequestEventType.CreateTradePending, (requestEvent) =>
        {
            Debug.Log(" ... PASSED: CREATE TRADE PENDING " + requestEvent.Data.id);
        });
        await tradeCreationHandler.RegisterCallback(RequestEventType.CreateTradeBroadcast, (requestEvent) =>
        {
            Debug.Log(" ... PASSED: CREATE TRADE BROADCAST " + requestEvent.Data.id);
        });
        await tradeCreationHandler.RegisterCallback(RequestEventType.CreateTradeExecuted, async (tradeCreateEvent) =>
        {
            Debug.Log(" ... PASSED: CREATE TRADE EXECUTED: " + tradeCreateEvent.Data.id);

            // Let the developer accept the tester's request for the trade.
            Debug.Log("(7/8) Completing trade from the testing account to the developer account ... ");
            string tradeId = tradeCreateEvent.Data.param1;
            IRequestHandler tradeAcceptHandler = adminIdentity.CompleteTradeItemRequestHandler(tradeId);
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradePending, (tradeCompleteEvent) =>
            {
                Debug.Log(" ... PASSED: COMPLETE TRADE PENDING " + tradeCompleteEvent.Data.id);
            });
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeBroadcast, (tradeCompleteEvent) =>
            {
                Debug.Log(" ... PASSED: COMPLETE TRADE BROADCAST " + tradeCompleteEvent.Data.id);
            });
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeExecuted, async (tradeCompleteEvent) =>
            {
                Debug.Log(" ... PASSED: COMPLETE TRADE EXECUTED: " + tradeCompleteEvent.Data.id);

                // Sequence lock to wait for successful trade of item before melting.
                Debug.Log("(8/8) Melting an item on the testing account ... ");
                IRequestHandler meltHandler = testerIdentity.CreateMeltItemRequestHandler(TESTING_ITEM_ID, 1);
                await meltHandler.RegisterCallback(RequestEventType.MeltPending, (requestEvent) =>
                {
                    Debug.Log(" ... PASSED: MELT PENDING " + requestEvent.Data.id);
                });
                await meltHandler.RegisterCallback(RequestEventType.MeltBroadcast, (requestEvent) =>
                {
                    Debug.Log(" ... PASSED: MELT BROADCAST " + requestEvent.Data.id);
                });
                await meltHandler.RegisterCallback(RequestEventType.MeltExecuted, async (meltCompleteEvent) =>
                {
                    Debug.Log(" ... PASSED: MELT EXECUTED " + meltCompleteEvent.Data.id);

                    // We've completed our final test.
                    Debug.Log("=== All tests executed successfully. ===");
                });
                await meltHandler.Execute();
            });
            await tradeAcceptHandler.Execute();
        });
        await tradeCreationHandler.Execute();
    }

    // Upon project start, execute the battery of Enjin SDK runtime function tests.
    async void Start()
    {
        Debug.Log("=== Executing Enjin SDK runtime tests. ===");
        EnjinLogger.SetDebugActive(DEBUG);
        

        Debug.Log("(1/8) Initializing the platform for use as game server ... ");
        EnjinConnector.Initialize(PLATFORM_URL);
        EnjinUser developerUser = await EnjinUser.Login(SERVER_EMAIL, SERVER_PASSWORD);
        Option<EnjinIdentity> developerIdentityOption = await developerUser.GetIdentityForAppId(APP_ID);
        EnjinIdentity developerIdentity = developerIdentityOption.Reduce(() =>
        {
            throw new Exception("Test failed: unable to retrieve developer identity.");
        });
        string developerAddress = developerIdentity.GetEthereumAddress().Reduce(() =>
        {
            throw new Exception("Test failed: the developer must have linked their identity to an Ethereum address.");
        });
        Debug.Log(developerAddress + ", " + developerIdentity.GetAppId());
        adminIdentity = developerIdentity.AsAdmin();
        Debug.Log(" ... PASSED.");

        Debug.Log("(2/8) Creating a new testing user ... ");
        long timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        string testName = "test" + timestamp;
        string testEmail = testName + "@mail.com";
        string testPassword = "test";
        await adminIdentity.CreateUser(testName, testEmail, testPassword, "Platform Owner");
        Debug.Log(" ... PASSED.");

        Debug.Log("(3/8) Create tester identity and verify login details ... ");
        await adminIdentity.CreateIdentity(testEmail, "", new List<EnjinIdentityField>());
        EnjinUser testerUser = await EnjinUser.Login(testEmail, testPassword);
        Option<EnjinIdentity> testerIdentityOption = await testerUser.GetIdentityForAppId(APP_ID);
        testerIdentity = testerIdentityOption.Reduce(() =>
        {
            throw new Exception("Test failed: unable to retrieve tester identity.");
        });
        Debug.Log(" ... PASSED.");

        string linkingCode = testerIdentity.GetLinkingCode();
        await testerIdentity.RegisterCallback(IdentityEventType.IdentityUpdated,
        async identityEventData =>
        {
            if (identityEventData.Event_Type.Equals("identity_linked"))
            {
                await testerIdentity.RefreshIdentity();
                EnjinLogger.ConsoleReporter(LoggerType.INFO, " ... PASSED.");

                Debug.Log("(5/8) Sending item to testing account ... ");

                // Retrieve the testing item's data.
                Option<List<EnjinCryptoItem>> itemOption = await adminIdentity.GetItem(TESTING_ITEM_ID);
                List<EnjinCryptoItem> itemIndices = itemOption.Reduce(() =>
                {
                    throw new Exception("Test failed: admin does not have access to the testing item.");
                });
                EnjinCryptoItem item = itemIndices[0];

                // Retrieve a count of the item's reserve.
                BigInteger reserveCount = item.Reserve;
                Debug.Log("Item has reserve count of " + reserveCount);

                // Retrieve the admin's balance of the item.
                int adminBalance = (int)(await adminIdentity.GetBalance(TESTING_ITEM_ID)).Reduce(() =>
                {
                    throw new Exception("Test failed: could not get admin testing item balance.");
                });
                Debug.Log("Admin has balance of " + adminBalance);

                // Retrieve the Ethereum address from the tester identity.
                string testerAddress = testerIdentity.GetEthereumAddress()
                    .Reduce(() =>
                    {
                        throw new Exception("Test failed: the testing identity must have an Ethereum address after linking.");
                    });

                // If there are unminted reserves of the testing token, use them.
                if (reserveCount != 0)
                {

                    // Mint an item from the administrator to our testing user.
                    IRequestHandler mintRequestHandler = adminIdentity.CreateMintItemRequestHandler(TESTING_ITEM_ID,
                         new string[] { testerAddress }, new int[] { 1 });
                    await mintRequestHandler.RegisterCallback(RequestEventType.MintPending, (requestEvent) =>
                    {
                        Debug.Log(" ... PASSED: MINT PENDING " + requestEvent.Data.id);
                    });
                    await mintRequestHandler.RegisterCallback(RequestEventType.MintBroadcast, (requestEvent) =>
                    {
                        Debug.Log(" ... PASSED: MINT BROADCAST " + requestEvent.Data.id);
                    });
                    await mintRequestHandler.RegisterCallback(RequestEventType.MintExecuted, async (requestEvent) =>
                    {
                        Debug.Log(" ... PASSED: MINT EXECUTED: " + requestEvent.Event_Data.Id);
                        await ContinueTesting(testerAddress, developerAddress);
                    });
                    await mintRequestHandler.Execute();
                }

                // If the developer wallet is unable to mint reward tokens from the reserve, try to send it from the developer wallet.
                else if (adminBalance > 0)
                {
                    IRequestHandler sendRequestHandler = adminIdentity.CreateSendItemRequestHandler(TESTING_ITEM_ID, testerAddress, 1);
                    await sendRequestHandler.RegisterCallback(RequestEventType.SendPending, (requestEvent) => // TODO: test this to see if TP works now.
                    {
                        Debug.Log(" ... PASSED: SEND PENDING " + requestEvent.Data.id);
                    });
                    await sendRequestHandler.RegisterCallback(RequestEventType.SendBroadcast, (requestEvent) =>
                    {
                        Debug.Log(" ... PASSED: SEND BROADCAST " + requestEvent.Data.id);
                    });
                    await sendRequestHandler.RegisterCallback(RequestEventType.SendExecuted, async (requestEvent) =>
                    {
                        Debug.Log(" ... PASSED: SEND EXECUTED: " + requestEvent.Event_Data.Id);
                        await ContinueTesting(testerAddress, developerAddress);
                    });
                    await sendRequestHandler.Execute();
                }

                // Otherwise there really is nothing of this token left for the developer to give out.
                else
                {
                    Debug.Log(" ... FAILED: NO RESERVE TO MINT OR BALANCE TO SEND.");
                }
            }
        });
        Debug.Log("(4/8) Establishing wallet link for the testing account. Please link with code " + linkingCode + " ... ");
    }
}
