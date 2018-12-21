using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FL.LigArchivar.MessageBox;
using FL.LigArchivar.Utilities;
using FL.LigArchivar.ViewModels;

namespace FL.LigArchivar
{
    /// <summary>
    /// Bootstrapper for this application. Builds up the Caliburn.Micro infrastructure.
    /// </summary>
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bootstrapper"/> class.
        /// </summary>
        public Bootstrapper()
        {
            LogManager.GetLog = type => new Log4NetLogger(type);
            Initialize();
        }

        /// <inheritdoc/>
        protected override void Configure()
        {
            container = new SimpleContainer();
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IMessageBox, XamlMessageBox>();
            container.PerRequest<ShellViewModel>();
        }

        /// <inheritdoc/>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // Show the main shell.
            DisplayRootViewFor<ShellViewModel>();
        }

        /// <inheritdoc/>
        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            return instance;
        }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var instances = container.GetAllInstances(service);
            return instances;
        }

        /// <inheritdoc/>
        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}
