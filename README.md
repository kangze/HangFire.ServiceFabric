# HangFire.ServiceFabric

Azure Service Fabric Stateful Service support for [Hangfire](http://hangfire.io/) library. By using this library you can store all jobs information in Azure Service Fabric.

base on Hangfire@1.7.3

# Installation
- You must download all source code to local because of project that have not publish to Nuget.org,
- Create a Service Fabric Application with a Stateful Service that reference Hangfire.ServiceFabric.StatefulService Project.
- Modifiy Stateful Service class make it inherit from HangfireStatefulService.
```
      internal sealed class HangfireStorage : HangfireStatefulService
      {
        public HangfireStorage(StatefulServiceContext context)
            : base(context, "_prefix")
        { }

        ...
       }
```
- The above code will automatic add some FabricTransportServiceRemotingListener

# Usage ASP.NET Core

- Add ASP.NET Core Application and reference Hangfire.ServiceFabric project.
- Modify Startup.cs

```
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UseServiceFabric("fabric:HangfireServiceFabricSfApp/HangfireStorage"));
            services.AddHangfireServer();
        }

         public void Configure(IApplicationBuilder app, IHostingEnvironment env)
         {
             app.UseHangfireDashboard();
         }
```
