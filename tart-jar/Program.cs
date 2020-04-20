using System;
using System.Threading.Tasks;
using CommandLine;

namespace tart_jar {
    class Program {
        static Task Main(string[] args) {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(Run, errors => Task.FromResult<Options>(null));
        }

        static async Task Run(Options options) {
            var fb = new FBScrapper(options.ImplicitWaitSeconds, options.PageLoadSeconds);

            try {
                await Login(fb);
            } finally {
                fb.Dispose();
            }

            async Task Login(FBScrapper fb) {
                if (options.AutoLogin) {
                    var result = await fb.LoginAsync(options.Id, options.Password);
                    if (!result) {
                        Console.WriteLine("login failed");
                        return;
                    }
                    Console.WriteLine("login succeed");
                } else {
                    Console.WriteLine("wait for login..");
                    await fb.WaitForLoginManuallyAsync();
                    Console.WriteLine("login succeed");
                }
            }

            async Task Scrap(FBScrapper fb) {
                var wait = Task.Run(() => {
                    Console.WriteLine("press enter when ready:");
                    Console.ReadLine();
                    Console.WriteLine("scrap begin");
                });

                await foreach (var data in fb.ScrapEventDataAsync(options.AppId, options.Since, options.Until, wait)) {

                }

                Console.WriteLine("scrap done");
            }
        }
    }

    public class Options {
        [Option("autologin", HelpText = "Requires id and password")]
        public bool AutoLogin { get; set; }

        [Option("id", HelpText = "email of facebook account")]
        public string Id { get; set; }
        [Option("pw", HelpText = "password of facebook account")]
        public string Password { get; set; }
        [Option("app", Required = true, HelpText = "app id from facebook analytics")]
        public string AppId { get; set; }

        [Option("since", Required = true)]
        public DateTime Since { get; set; }
        [Option("until", Required = true)]
        public DateTime Until { get; set; }

        [Option("wait", Default = 2f)]
        public double ImplicitWaitSeconds { get; set; }
        [Option("pageload", Default = 5f)]
        public double PageLoadSeconds { get; set; }
    }
}