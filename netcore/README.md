# Using TraceSpy for .NET Core (ASP or other) on Windows
.NET Core logging is not super easy to configure (this is the least to say...), especially in ASP.NET Core code. So I have provided a support .cs file that enables you to use ETW simple text traces (on Windows platform only) very easily as a logging provider under .NET core. Of course, you will then be able to get those traces in TraceSpy and WpfTraceSpy!

Just add the EventProvider.cs file somewhere in your project, and integrate the logger in your startup code like this:

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Just add this line (with a using TraceSpy; on top of the file)
            // It will only works on Windows, but will fail gracefully w/o error on other OSes
            // The guid is an arbitrary value that you will have to configure in (Wpf)TraceSpy's ETW providers
            loggerFactory.AddEventProvider(new Guid("a3f87db5-0cba-4e4e-b712-439980e59870"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
