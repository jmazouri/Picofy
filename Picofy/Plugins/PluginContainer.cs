using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Picofy.Plugins
{
    public class PluginContainer
    {
        private static PluginContainer _current;
        public static PluginContainer Current => _current ?? (_current = new PluginContainer());

        public const string PluginDirectory = "Plugins";

        public CompositionContainer Container { get; }

        public PluginContainer()
        {
            var registration = new RegistrationBuilder();

            registration.ForTypesDerivedFrom<BasicPlugin>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .Export<BasicPlugin>();

            bool tryAgain = true;

            while (tryAgain)
            {
                try
                {
                    DirectoryCatalog dircat = new DirectoryCatalog(PluginDirectory, registration);
                    tryAgain = false;

                    Container = new CompositionContainer(dircat, CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe);
                    Container.ComposeParts();
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(PluginDirectory);
                }
            }
        }
    }
}
