using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodingHelmet.Optional;
using CodingHelmet.Optional.Extensions;
using UnityEngine;
using EnjinSDK;
using PusherClient;

public class LoadNFTs : MonoBehaviour
{
    public EnjinCryptoItem unipig;
    private EnjinIdentity userIdentity;
    private Dictionary<string, string> TOKEN_IDS = new Dictionary<string, string>
    {
        ["Unipig"] = "18000000000010b7",
        ["Unicoin"] = "30000000000010b6"
    };

    public static LoadNFTs instance;
    private readonly int APP_ID = 5049;
    // Start is called before the first frame update
    void Start()
    { 
        
        Login();

       //loadItems();

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async public void Login()
    {
        if (instance== null)
        {
            instance = this;
        }
        EnjinConnector.Initialize("https://kovan.cloud.enjin.io/");
             
             
        EnjinUser user = await EnjinUser.Login("Simonrieger568@gmail.com", "7621hrrs");
        Option<EnjinIdentity> userIdentityOption = await user.GetIdentityForAppId(APP_ID);
       
        //TODO: ausgabe für den Benutzer falls es eine falsche Email ist etc

        userIdentity = userIdentityOption.Reduce(() =>
        {
            throw new SystemException("User Identity not found");
        });
        
        
        //Get Items
        
        //
        //Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>> totalInventory =await
        //   LoginManager.loginManagerInstance.userIdentity.GetAllItemsByApp();
         
        Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>> totalInventory =await
            userIdentity.GetAllItemsByApp();
        
        foreach ( int appID in totalInventory.Keys)
        {
            Debug.Log("foreach item with app id"+ appID);
            Dictionary<string, List<EnjinCryptoItem>> appInventory = totalInventory[appID];
            foreach (KeyValuePair<string, List<EnjinCryptoItem>> item in appInventory)
            {
                // item.key = item id        item.value == List<EnjinCryptoItem>
                Debug.Log("Item key"+item.Key+"item Value:"+item.Value);

                
                foreach (EnjinCryptoItem enjinCryptoItem in item.Value)
                {
                    Debug.Log("Crypto Item name:" + enjinCryptoItem.Name+"crypto item:"+enjinCryptoItem);
                    if (enjinCryptoItem.Token_ID.Equals("18000000000010b7"));
                    {
                        unipig = enjinCryptoItem;

                    }

                }
            }
        }
        
    }
    

    async public void loadItems()
    {
        //Get Items
        
        //
        Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>> totalInventory =await
          LoginManager.loginManagerInstance.userIdentity.GetAllItemsByApp();
         
        //Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>> totalInventory =await
          //  userIdentity.GetAllItemsByApp();
        
        foreach ( int appID in totalInventory.Keys)
        {
            Debug.Log("foreach item with app id"+ appID);
            Dictionary<string, List<EnjinCryptoItem>> appInventory = totalInventory[appID];
            foreach (KeyValuePair<string, List<EnjinCryptoItem>> item in appInventory)
            {
                // item.key = item id        item.value == List<EnjinCryptoItem>
                Debug.Log("Item key"+item.Key+"item Value:"+item.Value);

                
                foreach (EnjinCryptoItem enjinCryptoItem in item.Value)
                {
                    Debug.Log("Crypto Item name:" + enjinCryptoItem.Name+"crypto item:"+enjinCryptoItem);
                    
                    
                }
            }
        }
        
        
        
        
    }
    
    
}
