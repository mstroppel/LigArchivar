using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
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
            Initialize();
        }

        /// <inheritdoc/>
        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();

            container.PerRequest<ShellViewModel>();
        }

        /// <inheritdoc/>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        /// <inheritdoc/>
        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        /// <inheritdoc/>
        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}
