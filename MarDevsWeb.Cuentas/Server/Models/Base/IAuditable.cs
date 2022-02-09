using System;
using System.Collections.Generic;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public interface IAuditable
    {
        int CreadoPor { get;set;}
        DateTime CreadoEl { get;set;}      
        
    }
}
