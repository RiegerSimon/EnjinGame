using CodingHelmet.Optional;
using EnjinSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class LoginManager : MonoBehaviour
{
    public GameObject emailInput;
    public GameObject passwordInput;
    public TextMeshProUGUI errorfield;
    public GameObject gameSceneChanger;

    public static LoginManager loginManagerInstance;
    
    
    public EnjinIdentity userIdentity = null;
    public EnjinAdminIdentity adminIdentity = null;
    public string adminAddress = "";

    private readonly string PLATFORM_URL = "https://kovan.cloud.enjin.io/"; 
    private readonly int APP_ID = 5049; 
    // Start is called before the first frame update
    void Start()
    {
        if (loginManagerInstance== null)
        {
            loginManagerInstance = this;
        }
        EnjinConnector.Initialize(PLATFORM_URL);
        AdminLogin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    async public void AdminLogin()
    {
        string email = "simonrieger2002@gmail.com";
        string password = "7621hrrs";
        EnjinUser user = await EnjinUser.Login(email, password);
        Option<EnjinIdentity> adminIdentityOption = await user.GetIdentityForAppId(APP_ID);

        EnjinIdentity identity = adminIdentityOption.Reduce(() =>
        {
            
            errorfield.SetText("User Identity not found");
            throw new SystemException("User Identity not found");
           
        });
        adminAddress = identity.GetEthereumAddress().Reduce("");
        adminIdentity = identity.AsAdmin();
    }

    async public void Login()
    {
        string email = emailInput.GetComponent<InputField>().text;
        string password = passwordInput.GetComponent<InputField>().text;
        errorfield.SetText("");
        EnjinUser user = await EnjinUser.Login(email, password);
        Option<EnjinIdentity> userIdentityOption = await user.GetIdentityForAppId(APP_ID);
       
        //TODO: ausgabe für den Benutzer falls es eine falsche Email ist etc
       
        userIdentity = userIdentityOption.Reduce( () =>
        {
            errorfield.SetText("User Identity not found");
            throw new SystemException("User Identity not found");
            
            

        });
        
        //Todo:  new muss weg fehler
        //Game scene
        //new GameSceneChanger().GoToMainGame();
        gameSceneChanger.GetComponent<GameSceneChanger>().GoToMainGame();
        
    }

    
    
}

