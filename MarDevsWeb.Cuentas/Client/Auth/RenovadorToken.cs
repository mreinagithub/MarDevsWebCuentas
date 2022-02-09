using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MarDevsWeb.Cuentas.Client.Auth
{
    public class RenovadorToken : IDisposable
    {
        Timer timer;
        private readonly ILoginService loginService;

        public RenovadorToken(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        public void Iniciar()
        {
            timer = new Timer();
            timer.Interval = 1000 * 60 * 4; //4 Minutos
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            loginService.ManejarRenovarToken();            
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
