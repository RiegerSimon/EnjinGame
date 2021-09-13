using System;
using System.Collections;
using System.Collections.Generic;
using Unisave;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Utils;

public class AdminAuth : Entity
{
    /// <summary>
    /// Replace this with your own entity attributes
    /// </summary>
    
    public string myAttribute = "Default value";
    
    
    // create hash (during registration)
    string hashedPassword = Hash.Make("password");

// compare value against a hash (during login)
    string providedPassword = "password";
    //bool matches = Hash.Check(providedPassword, hashedPassword); // true
}
