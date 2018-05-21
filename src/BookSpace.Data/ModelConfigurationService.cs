using System;
using System.Linq;
using System.Reflection;
using BookSpace.Data.Contracts;
using BookSpace.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Data
{
    public class ModelConfigurationService : IModelConfigurationService
    {
        public void ConfigureModels(ModelBuilder builder)
        {
            string assemblyName = "BookSpace.Models";
            string modelsNamespace = "BookSpace.Models";
            string configurationsNamespace = "BookSpace.Models.Configurations";

            Assembly assembly = Assembly.Load(assemblyName);

            Type[] modelTypes = assembly
                                    .GetTypes()
                                    .Where(t => t.Namespace == modelsNamespace)
                                    .ToArray();

            Type[] configurationTypes = assembly
                                            .GetTypes()
                                            .Where(t => t.Namespace == configurationsNamespace)
                                            .ToArray();

            foreach (Type modelType in modelTypes)
            {
                Type configurationType = configurationTypes.SingleOrDefault(n => n.Name == modelType.Name + "Configuration");

                if (configurationType != null)
                {
                    MethodInfo configureMethod = this.GetType().GetMethod("Configure", BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo genericConfig = configureMethod.MakeGenericMethod(modelType, configurationType);

                    genericConfig.Invoke(this, new object[] { builder });
                }
            }
        }

        private void Configure<TModel, TModelConfig>(ModelBuilder modelBuilder)
            where TModel : class
            where TModelConfig : IEntityTypeConfiguration<TModel>
        {
            IEntityTypeConfiguration<TModel> configuration = Activator.CreateInstance<TModelConfig>();
            modelBuilder.ApplyConfiguration(configuration);
        }
    }
}
