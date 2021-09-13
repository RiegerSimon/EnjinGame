using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnjinSDK;

public class StoreController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async public void PurchaseUniCoin()
    {
        string coinTokenId = "30000000000010b6";
        string enjinId = "0";

        string adminAddress = LoginManager.loginManagerInstance.adminAddress;

        EnjinItemParameter [] sendItems= new EnjinItemParameter[]{ EnjinItemParameter.ConstructFungible(enjinId, 1)};
        EnjinItemParameter [] receiveItems= new EnjinItemParameter[]{EnjinItemParameter.ConstructFungible(coinTokenId, 10)};

        IRequestHandler tradeHandler = LoginManager.loginManagerInstance.userIdentity.
            CreateTradeItemRequestHandler(sendItems,receiveItems,adminAddress);

        await tradeHandler.RegisterCallback(RequestEventType.CreateTradePending, requestEvent =>
        {
            Debug.Log("Create Trade Request |Pending Done|");
        });
        
        await tradeHandler.RegisterCallback(RequestEventType.CreateTradeBroadcast, requestEvent =>
        {
            Debug.Log("Create Trade Request |Broadcast Done|");
        });
        
        await tradeHandler.RegisterCallback(RequestEventType.CreateTradeExecuted, async requestEvent =>
        {
            string tradeID = requestEvent.Data.param1;
            Debug.Log("Create Trade Request |Execution Done|");
            
            //accept the trade
            IRequestHandler tradeAcceptHandler = LoginManager.loginManagerInstance.adminIdentity.CompleteTradeItemRequestHandler(tradeID);

            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradePending, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Pending Done|");
            });
            
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeBroadcast, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Broadcast Done|");
            });
            
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeExecuted, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Execution Done| ~ Trade is done");
            });
            await tradeAcceptHandler.Execute();
        });
        await tradeHandler.Execute();
    }
    
    async public void PurchaseUniPigCollection1()
    {
        string coinTokenId = "30000000000010b6";
        string uniPigC1 = "18000000000010b7";

        string adminAddress = LoginManager.loginManagerInstance.adminAddress;

        EnjinItemParameter [] sendItems= new EnjinItemParameter[]{ EnjinItemParameter.ConstructFungible(coinTokenId, 10)};
        EnjinItemParameter [] receiveItems= new EnjinItemParameter[]{EnjinItemParameter.ConstructFungible(uniPigC1, 1)};

        IRequestHandler tradeHandler =
            LoginManager.loginManagerInstance.userIdentity.CreateTradeItemRequestHandler(sendItems, receiveItems, adminAddress);
        
       
            
        await tradeHandler.RegisterCallback(RequestEventType.CreateTradePending, requestEvent =>
        {
            Debug.Log("Create Trade Request |Pending Done|");
        });
        
        await tradeHandler.RegisterCallback(RequestEventType.CreateTradeBroadcast, requestEvent =>
        {
            Debug.Log("Create Trade Request |Broadcast Done|");
        });
        
        await tradeHandler.RegisterCallback(RequestEventType.CreateTradeExecuted, async requestEvent =>
        {
            string tradeID = requestEvent.Data.param1;
            Debug.Log("Create Trade Request |Execution Done|");
            
            //accept the trade
            IRequestHandler tradeAcceptHandler = LoginManager.loginManagerInstance.adminIdentity.CompleteTradeItemRequestHandler(tradeID);

            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradePending, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Pending Done|");
            });
            
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeBroadcast, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Broadcast Done|");
            });
            
            await tradeAcceptHandler.RegisterCallback(RequestEventType.CompleteTradeExecuted, completeEvent =>
            {
                Debug.Log("Complete Trade Request |Execution Done| ~ Trade is done");
            });

            await tradeAcceptHandler.Execute();
        });

        await tradeHandler.Execute();
    }
    
    
    
    
}
