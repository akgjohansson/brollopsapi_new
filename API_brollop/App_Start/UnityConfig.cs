using API_brollop.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace API_brollop
{
    public static class UnityConfig
    {

        public static void RegisterInstances()
        {
            var container = new UnityContainer();
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);

            container.RegisterType<ISpotifyApi, SpotifyApi>();
            container.RegisterInstance(new SpotifyApi());
        }
    }
}