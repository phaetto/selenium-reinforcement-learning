namespace SeleniumReinforcementLearning
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using OpenQA.Selenium.Support.UI;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless"); // Not compatible with document.elementFromPoint?
            chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";// @"C:\Program Files (x86)\Chromium\Application\chrome.exe";
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                try
                {
                    Console.WriteLine("\nLoading the environment...");
                    driver.Manage().Window.Size = new Size(1000, 768);
                    var random = new Random(1);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);

                    // Login to the site, so we don't have to 'discover' this (for now)
                    //Console.WriteLine("Login in for the first time...");
                    //driver.Navigate().GoToUrl("https://intraactiveamatest.sharepoint.com/sites/MessagesRLUITest");
                    //wait.UntilElementVisible(By.CssSelector("input[name='loginfmt'][type='email']"));

                    //var emailInput = driver.FindElement(By.CssSelector("input[name='loginfmt'][type='email']"));
                    //emailInput.SendKeys("ama@intraactiveamatest.onmicrosoft.com");
                    //wait.UntilElementVisible(By.CssSelector("input[type='submit']"));
                    //driver.FindElement(By.CssSelector("input[type='submit']")).Click();

                    //wait.UntilElementVisible(By.CssSelector("input[name='passwd'][type='password']"));
                    //var passwordInput = driver.FindElement(By.CssSelector("input[name='passwd'][type='password']"));
                    //passwordInput.SendKeys("qaz123WSX!@#$");
                    //wait.UntilElementVisible(By.CssSelector("input[type='submit']"));
                    //driver.FindElement(By.CssSelector("input[type='submit']")).Click();

                    // Start training
                    var seleniumEnvironment = new IntraActiveAmaTestSeleniumEnvironment(
                        driver,
                        "https://intraactive-sdk-ui.azurewebsites.net/");
                    var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<IWebElement>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    // Execute
                    Console.WriteLine("Training...");
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    await rlTrainer.Run(epochs: 5);
                    stopWatch.Stop();
                    Console.WriteLine($"\tDone training ({stopWatch.Elapsed.TotalSeconds} sec)");

                    Console.WriteLine("Walk to goal...");
                    var initialState = await seleniumEnvironment.GetInitialState();
                    var path = await rlTrainer.Walk(initialState, goalCondition: async (s, a) => await seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    Console.WriteLine("To reach the goal you need to:");
                    foreach (var pair in path)
                    {
                        Console.WriteLine($"\t from {pair.State.ToString()}");
                        Console.WriteLine($"\t\t{pair.Action.ToString()}");
                    }
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }

                Console.WriteLine("\nDone.");
                Console.ReadLine();
                Console.WriteLine("(Unloading...)");
            }
        }

        private class IntraActiveAmaTestSeleniumEnvironment : SeleniumEnvironment
        {
            private const string RemoveStyleSheetsScript = @"
var stylesheets = document.getElementsByTagName('link');

while (stylesheets.length) {
    var sheet = stylesheets[0];
    sheet.parentNode.removeChild(sheet);
}";
            private const string RemoveStyleTagsScript = @"
function removeStyles(el) {
    el.removeAttribute('style');
    if(el.childNodes.length > 0) {
        for(var child in el.childNodes) {
            if(el.childNodes[child].nodeType == 1)
                removeStyles(el.childNodes[child]);
        }
    }
}

removeStyles(document.body);
";
            private const string RemoveElementStylesScript = @"
var styles = document.getElementsByTagName('style');

while(styles.length) {
    var style = styles[0];
    style.parentNode.removeChild(style);
}
";

            private readonly WebDriverWait wait;
            private readonly RemoteWebDriver webDriver;
            private readonly string url;

            public IntraActiveAmaTestSeleniumEnvironment(RemoteWebDriver webDriver, string url) : base(webDriver, url, null)
            {
                this.wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                this.webDriver = webDriver;
                this.url = url;
            }
            public override async Task<State<IReadOnlyCollection<IWebElement>>> GetInitialState()
            {
                webDriver.Navigate().GoToUrl(url);
                wait.UntilUrl(url);

                // F-bug
                webDriver.ExecuteScript("window.localStorage.setItem", new string[] {
                    "navigator-1",
                    "JSON.stringify({'version':{'id':13,'title':'x-mas','public':true},'design':{'menuBar':{'backgroundColor':'#41585b','iconColor':'#ffffff','logo':'https://intraactivedev.sharepoint.com/sites/IntraActive-Admin/Lists/IAImages/Logos/Full.png','layout':'vertical','hideHubMenu':true,'logoLinkUrl':'https://bdoscanrevisionas.sharepoint.com/sites/work','hideLocalMenu':false,'highlightColor':'#57757a'},'topItems':{'textColor':'#ffffff','backgroundColor':'#314545','highlightColor':'#293939','font':'Trebuchet MS, Helvetica, sans-serif','fontSize':'14px','fontWeight':'100','textTransfrom':'none','letterSpacing':'0px','showLine':true,'lineThickness':'1px','textTransform':'uppercase','iconSize':20,'iconBackgroundColor':null},'elementGroups':{'backgroundColor':'#f3f3f3','textColor':'#333333','font':'Trebuchet MS, Helvetica, sans-serif','fontSize':'18px','fontWeight':'600','textTransfrom':'none','letterSpacing':'0px','showLine':true,'lineThickness':'1px','iconSize':25,'iconBackgroundColor':null,'iconColor':'#4a90e2'},'elements':{'textColor':'#333333','iconColor':'#1d614f','iconBackgroundColor':'#98002e','font':'Trebuchet MS, Helvetica, sans-serif','fontSize':'16px','fontWeight':'400','textTransfrom':'none','letterSpacing':'0px','iconSize':47}},'megamenu':{'topItems':[{'config':{'title':'Top item 1','icon':'Car'},'elementGroups':[[{'config':{'headline':'Advisory','url':'https://bdoscanrevisionas.sharepoint.com/sites/Advisory2'},'elements':[{'type':'link','config':{'text':'Consulting','url':'https://bdoscanrevisionas.sharepoint.com/sites/Advisory2/SitePages/Consulting.aspx?web=1'},'targeting':null},{'type':'link','config':{'text':'Corporate Finance'},'targeting':null},{'type':'link','config':{'text':'Risk Assurance'},'targeting':{'enabled':false,'operator':'AND','department':null,'jobTitle':null,'location':null,'custom1':null,'custom2':null}}],'targeting':null,'type':null}],[{'config':{'headline':'Kontorsteder'},'elements':[{'type':'link','config':{'text':'Bogense','url':'https://bdoscanrevisionas.sharepoint.com/sites/Odense/SitePages/Bogense.aspx'},'targeting':null},{'type':'link','config':{'text':'Brønderslev','url':'https://bdoscanrevisionas.sharepoint.com/sites/Brnderslev'},'targeting':null},{'type':'link','config':{'text':'Ejstrupholm'},'targeting':null},{'type':'link','config':{'text':'Esbjerg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Esbjerg2'},'targeting':null},{'type':'link','config':{'text':'Frederikshavn','url':'https://bdoscanrevisionas.sharepoint.com/sites/Frederikshavn'},'targeting':null},{'type':'link','config':{'text':'Faaborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Faaborg/'},'targeting':null},{'type':'link','config':{'text':'Haderslev','url':'https://bdoscanrevisionas.sharepoint.com/sites/Haderslev'},'targeting':null},{'type':'link','config':{'text':'Herning','url':'https://bdoscanrevisionas.sharepoint.com/sites/Herning'},'targeting':null},{'type':'link','config':{'text':'Hirtshals','url':'https://bdoscanrevisionas.sharepoint.com/sites/Hirtshals'},'targeting':null},{'type':'link','config':{'text':'Hjørring','url':'https://bdoscanrevisionas.sharepoint.com/sites/Hjrring'},'targeting':null},{'type':'link','config':{'text':'Hobro','url':'https://bdoscanrevisionas.sharepoint.com/sites/Hobro'},'targeting':null},{'type':'link','config':{'text':'Kalundborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Kalundborg2'},'targeting':null},{'type':'link','config':{'text':'Kolding','url':'https://bdoscanrevisionas.sharepoint.com/sites/Kolding'},'targeting':null},{'type':'link','config':{'text':'København','url':'https://bdoscanrevisionas.sharepoint.com/sites/Kobenhavn'},'targeting':null},{'type':'link','config':{'text':'Middelfart','url':'https://bdoscanrevisionas.sharepoint.com/sites/Middelfart'},'targeting':null},{'type':'link','config':{'text':'Nykøbing Mors','url':'https://bdoscanrevisionas.sharepoint.com/sites/NykbingMors'},'targeting':null},{'type':'link','config':{'text':'Nyborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Odense/SitePages/Nyborg.aspx'},'targeting':null},{'type':'link','config':{'text':'Odense','url':'https://bdoscanrevisionas.sharepoint.com/sites/Odense'},'targeting':null},{'type':'link','config':{'text':'Randers','url':'https://bdoscanrevisionas.sharepoint.com/sites/Randers'},'targeting':null},{'type':'link','config':{'text':'Roskilde','url':'https://bdoscanrevisionas.sharepoint.com/sites/Roskilde'},'targeting':null},{'type':'link','config':{'text':'Silkeborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Silkeborg'},'targeting':null},{'type':'link','config':{'text':'Skagen','url':'https://bdoscanrevisionas.sharepoint.com/sites/Skagen'},'targeting':null},{'type':'link','config':{'text':'Skive','url':'https://bdoscanrevisionas.sharepoint.com/sites/Skive'},'targeting':null},{'type':'link','config':{'text':'Svendborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Svendborg'},'targeting':null},{'type':'link','config':{'text':'Sæby','url':'https://bdoscanrevisionas.sharepoint.com/sites/Sby'},'targeting':null},{'type':'link','config':{'text':'Varde','url':'https://bdoscanrevisionas.sharepoint.com/sites/Varde'},'targeting':null},{'type':'link','config':{'text':'Vejle','url':'https://bdoscanrevisionas.sharepoint.com/sites/Vejle'},'targeting':null},{'type':'link','config':{'text':'Viborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Viborg'},'targeting':null},{'type':'link','config':{'text':'Ølgod','url':'https://bdoscanrevisionas.sharepoint.com/sites/Olgod'},'targeting':null},{'type':'link','config':{'text':'Aabybro','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aabybro'},'targeting':null},{'type':'link','config':{'text':'Aalborg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aalborg2'},'targeting':null},{'type':'link','config':{'text':'Aarhus','url':'https://bdoscanrevisionas.sharepoint.com/sites/aarhus'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Netværk og udvalg','url':'https://bdoscanrevisionas.sharepoint.com/sites/Netvaerkogudvalg'},'elements':[{'type':'link','config':{'text':'Forretningsudvalg'},'targeting':null},{'type':'link','config':{'text':'HR-ansvarlige'},'targeting':null},{'type':'link','config':{'text':'Salgsansvarlige'},'targeting':null},{'type':'link','config':{'text':'SAMU'},'targeting':null},{'type':'link','config':{'text':'Økonomiansvarlige'},'targeting':null},{'type':'link','config':{'text':'Valgudvalg'},'targeting':null},{'type':'link','config':{'text':'APT-revisionsansvarlige'},'targeting':null},{'type':'link','config':{'text':'BSO-partnere'},'targeting':null},{'type':'link','config':{'text':'BSO-ambassadører'},'targeting':null},{'type':'link','config':{'text':'CW superbrugere'},'targeting':null},{'type':'link','config':{'text':'Revisionsansvarligere partnere'},'targeting':null},{'type':'link','config':{'text':'Stamdata superbrugere'},'targeting':null},{'type':'link','config':{'text':'Faglige ambassadører'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Offentlig revision','url':'https://bdoscanrevisionas.sharepoint.com/sites/Offentlig'},'elements':[{'type':'link','config':{'text':'Sjælland'},'targeting':null},{'type':'link','config':{'text':'Midt/Nord'},'targeting':null},{'type':'link','config':{'text':'Fyn/Syd'},'targeting':null},{'type':'link','config':{'text':'Offentlige selskaber'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Privat revision','url':'https://bdoscanrevisionas.sharepoint.com/sites/PrivatRevision'},'elements':[{'type':'link','config':{'text':'Nord','url':'https://bdoscanrevisionas.sharepoint.com/sites/PrivatRevision/SitePages/REGION-NORD.aspx'},'targeting':null},{'type':'link','config':{'text':'Midt'},'targeting':null},{'type':'link','config':{'text':'Syddanmark'},'targeting':null},{'type':'link','config':{'text':'Sjælland'},'targeting':null}],'targeting':null,'type':null}]],'targeting':null,'type':'menu'},{'config':{'title':'Top item','icon':'Champagne'},'elementGroups':[[{'config':{'headline':'Om BDO','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO'},'elements':[{'type':'link','config':{'text':'Historien','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO','icon':'Balloons'},'targeting':null},{'type':'link','config':{'text':'Navnet','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO'},'targeting':null}],'targeting':{'enabled':false,'operator':'AND','department':null,'jobTitle':null,'location':null,'custom1':null,'custom2':null},'type':null}],[{'config':{'headline':'BDO Global','url':'https://bdoscanrevisionas.sharepoint.com/sites/BDOGlobal'},'elements':[{'type':'link','config':{'text':'Om BDO international','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/'},'targeting':null},{'type':'link','config':{'text':'Adgang til connect'},'targeting':null},{'type':'link','config':{'text':'Internationale kontakter'},'targeting':null},{'type':'link','config':{'text':'Konflikttjek'},'targeting':null},{'type':'link','config':{'text':'Referred work'},'targeting':null},{'type':'link','config':{'text':'Udstationering'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Ledelsesorganer','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Ledelsesorganer.aspx'},'elements':[{'type':'link','config':{'text':'Aktionærråd','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Aktion%C3%A6rr%C3%A5d.aspx'},'targeting':null},{'type':'link','config':{'text':'Bestyrelse og FU','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Bestyrelse.aspx'},'targeting':null},{'type':'link','config':{'text':'Risk Management (RMT)','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Risk-Management-(RMT).aspx'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Organisation','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Organisation.aspx'},'elements':[{'type':'link','config':{'text':'Organisationsdiagram','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Organisationsdiagram.aspx'},'targeting':null},{'type':'link','config':{'text':'Lokale ansvarlige','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Lokale-ansvarlige.aspx'},'targeting':null},{'type':'link','config':{'text':'Regionsledere','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Regionsledere.aspx?web=1'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Strategi','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Strategi.aspx'},'elements':[{'type':'link','config':{'text':'Et naturligt valg','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/BDO%27s-strategi--Et-naturligt-valg.aspx?web=1'},'targeting':null},{'type':'link','config':{'text':'Vision og mission','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Mission-og-Vision-for-BDO.aspx'},'targeting':null},{'type':'link','config':{'text':'Kodeks for indlevelse','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Kodeks-for-indlevelse.aspx'},'targeting':null},{'type':'link','config':{'text':'Kodeks for god ledelse','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/Kodeks-for-god-ledelse.aspx'},'targeting':null},{'type':'link','config':{'text':'BDO's ledelsesmodel','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmBDO/SitePages/BDO's-ledelsesmodel.aspx'},'targeting':null}],'targeting':null,'type':null}]],'targeting':null,'type':null},{'config':{'title':'Top item 3','icon':'Coins'},'elementGroups':[[{'config':{'headline':'Redaktør undervisning'},'elements':[{'type':'link','config':{'text':'Rasmus','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Rasmus','openInNewWindow':true},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Anja','text':'Anja'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Ditte','text':'Ditte'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Ellen','text':'Ellen'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Hanah','text':'Hannah'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Jeppe','text':'Jeppe','openInNewWindow':false},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Karin','text':'Karin'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Marianne','text':'Marianne'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Nina','text':'Nina'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Simon','text':'Simon'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Intern Undervisning'},'elements':[{'type':'link','config':{'text':'Anne','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Anne'},'targeting':null},{'type':'link','config':{'text':'Inger','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Inger'},'targeting':null},{'type':'link','config':{'text':'Karin Juul','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-KarinJuul'},'targeting':null},{'type':'link','config':{'text':'Simon Riis','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Simon2'},'targeting':null},{'type':'link','config':{'text':'Sofie','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Sofie'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Sophie','text':'Sophie'},'targeting':null},{'type':'link','config':{'text':'Line','url':'https://bdoscanrevisionas.sharepoint.com/sites/LineEgtoft'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Admin Undervisning'},'elements':[{'type':'link','config':{'text':'Admin SImon','url':'https://bdoscanrevisionas.sharepoint.com/sites/Simon-undervisning'},'targeting':null},{'type':'link','config':{'text':'Rasmus2','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Rasmus2','openInNewWindow':false},'targeting':null},{'type':'link','config':{'text':'Nicholai','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Nicholai'},'targeting':null},{'type':'link','config':{'text':'Mette','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Mette'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-RasmusIT','text':'Rasmus'},'targeting':null},{'type':'link','config':{'text':'Jonas','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Jonas'},'targeting':null},{'type':'link','config':{'text':'Seje Simon','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning_Seje-Simon'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Marianne2','text':'Marianne'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Nyttige links'},'elements':[{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/work/Billeder/Forms/AllItems.aspx','text':'Billeder','openInNewWindow':true},'targeting':null},{'type':'link','config':{'text':'IntraActive Admin (Navigator)','url':'https://bdoscanrevisionas.sharepoint.com/sites/IntraActive-Admin?component=IntraActive-NavigatorAdmin','openInNewWindow':true},'targeting':null},{'type':'link','config':{'text':'Om Work/ Redaktørportal','url':'https://bdoscanrevisionas.sharepoint.com/sites/OmWork','openInNewWindow':true},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Mega Menu Test'},'elements':[{'type':'link','config':{'text':'Test','url':'https://bdoscanrevisionas.sharepoint.com/sites/Undervisning-Rasmus/','openInNewWindow':true},'targeting':null}],'targeting':null,'type':null}]],'targeting':null,'type':null},{'config':{'title':'Top item 4','url':'','icon':'Trophy'},'elementGroups':[[{'config':{'headline':'Kalender','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Kalender.aspx'},'elements':[{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/_layouts/15/Events.aspx?ListGuid=4cb0c976-3e69-4b69-ac22-46d2f0af6daf','text':'Begivenheder'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Økonomi','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Oekonomi.aspx'},'elements':[{'type':'link','config':{'text':'Årsrapporter','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Aarsrapporter.aspx'},'targeting':null},{'type':'link','config':{'text':'Budgetter','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Budgetter.aspx'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Bestyrelsen','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Bestyrelse.aspx'},'elements':[{'type':'link','config':{'text':'Dagsordner fra bestyrelsesmøder','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Dagsordner-fra-bestyrelsesm%C3%B8der.aspx?web=1'},'targeting':null},{'type':'link','config':{'text':'Referater fra bestyrelsesmøder','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Bestyrelsesmoder-referater.aspx'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Partner-leder/SitePages/Partner-leder-seminar.aspx','headline':'Partner-/lederseminar'},'elements':[],'targeting':null,'type':null}]],'targeting':null,'type':'menu'},{'config':{'title':'Aktionærweb','url':'','icon':'Airplane'},'elementGroups':[[{'config':{'headline':'Forside','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb'},'elements':[{'type':'link','config':{'text':'Aktionærrådsmøder','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Aktion%C3%A6rr%C3%A5dsm%C3%B8der.aspx'},'targeting':null},{'type':'link','config':{'text':'Årsrapporter','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Arsrapporter.aspx'},'targeting':null},{'type':'link','config':{'text':'Budgetter','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Budgetter.aspx'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Ejeraftales%C3%A6ttet.aspx','text':'Ejeraftalesættet'},'targeting':null},{'type':'link','config':{'text':'Opkøb i BDO','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Opk%C3%B8b-i-BDO.aspx'},'targeting':null},{'type':'link','config':{'url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/%C3%98vrige.aspx','text':'Øvrige'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Aktionærmøder','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/Aktion%C3%A6rm%C3%B8der-og-generalforsamlinger.aspx'},'elements':[{'type':'link','config':{'text':'BDO Holding III','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/BDO-Holding-III,-Statsautoriseret-revisionsaktieselskab.aspx'},'targeting':null},{'type':'link','config':{'text':'BDO Holding IV','url':'https://bdoscanrevisionas.sharepoint.com/sites/Aktionrweb/SitePages/BDO-Holding-IV,-Statsautoriseret-revisionsaktieselskab.aspx'},'targeting':null}],'targeting':null,'type':null}]],'targeting':{'enabled':true,'operator':'OR','department':[{'label':'Aarhus','id':'fd2906bd-eb7f-46ed-85a8-5b34b672beea','field':'IATargetingDepartment'},{'label':'Berlin','id':'6805f9f8-75b0-41cd-b690-6c8570d1f978','field':'IATargetingDepartment'},{'label':'Copenhagen','id':'5af54754-76ca-4913-9205-ea35cf192f3a','field':'IATargetingDepartment'},{'label':'Dubai','id':'2ce1744b-52ad-4c14-a7e8-191bbbd9bbcd','field':'IATargetingDepartment'},{'label':'London','id':'6fa4ddf1-673d-4221-9613-96bb50f94448','field':'IATargetingDepartment'},{'label':'Madrid','id':'f5eda17a-80a4-4c51-80e7-ecf4958d0489','field':'IATargetingDepartment'},{'label':'New York','id':'4f3dc57e-d385-46e5-933c-afc68397dc22','field':'IATargetingDepartment'},{'label':'Paris','id':'6403d5e4-c568-4416-8765-9fe5500df768','field':'IATargetingDepartment'},{'label':'Rome','id':'a7753bd3-2dc0-4fcf-b10f-7077c0617ea5','field':'IATargetingDepartment'},{'label':'Test','id':'726fd1df-0852-4a5b-9543-3d95f1e9a5ea','field':'IATargetingDepartment'},{'label':'Tokyo','id':'8a8cc59a-821e-43c0-a0f2-ccccdee44606','field':'IATargetingDepartment'}],'jobTitle':[],'location':[{'label':'Brazil','id':'b8da09e4-5dd0-4487-9aa7-9419c97301f5','field':'IATargetingLocation'},{'label':'Belo Horizonte','id':'68ed057f-1be7-467f-9456-8d7edbf6ecbd','field':'IATargetingLocation'},{'label':'Rio','id':'fde0c798-744a-4360-9ee4-973f5a56bc50','field':'IATargetingLocation'},{'label':'Rio de Janeiro','id':'cfcb7f87-2016-4ff2-a1df-eda93da0a7fc','field':'IATargetingLocation'},{'label':'São Paulo','id':'afb4d88a-f82e-4926-bd81-9800fa59a695','field':'IATargetingLocation'},{'label':'Copenhagen','id':'2f8151aa-a4bb-4532-8502-b510ff58b3e4','field':'IATargetingLocation'},{'label':'Denmark','id':'9126287c-22ae-445e-9453-4d4e9860e84b','field':'IATargetingLocation'},{'label':'Aarhus','id':'4c7266a9-32fd-44ef-8399-26133e8d466a','field':'IATargetingLocation'},{'label':'Copenhagen','id':'99340aa6-c8e7-413a-9022-046ed872e809','field':'IATargetingLocation'},{'label':'São Paulo','id':'ce98da1e-519a-42e4-9507-bb04e9e53787','field':'IATargetingLocation'}],'custom1':[],'custom2':[]},'type':'menu'},{'config':{'icon':'Book','title':'Top item 5'},'elementGroups':[[{'config':{'icon':'Book','headline':'Test','imageUrl':'https://intraactivedev.sharepoint.com/sites/IntraActive-NavigatorAdmin/lists/IANavigatorImages/Full.png'},'elements':[],'targeting':null,'type':null}]],'targeting':null,'type':null},{'config':{'title':'Information','icon':'Bookmark'},'elementGroups':[[{'config':{'headline':'BDO Academy','url':'https://bdoscanrevisionas.sharepoint.com/sites/BDO_LMS365_Learning'},'elements':[{'type':'link','config':{'text':'Dashboard','url':'https://bdoscanrevisionas.sharepoint.com/sites/BDO_LMS365_Learning/SitePages/Dashboard.aspx'},'targeting':null},{'type':'link','config':{'text':'Kalender','openInNewWindow':true,'url':'https://bdoscanrevisionas.sharepoint.com/sites/BDO_LMS365_Learning/SitePages/CourseCatalog.aspx'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Brancher','url':'https://bdoscanrevisionas.sharepoint.com/sites/Brancher2'},'elements':[{'type':'link','config':{'text':'Auto','url':'https://bdoscanrevisionas.sharepoint.com/sites/auto'},'targeting':null},{'type':'link','config':{'text':'Ejendomme','url':'https://bdoscanrevisionas.sharepoint.com/sites/Ejendomme'},'targeting':null},{'type':'link','config':{'text':'Energi og forsyning','url':'https://bdoscanrevisionas.sharepoint.com/sites/Energiogforsyning'},'targeting':null},{'type':'link','config':{'text':'Fiskeri','url':'https://bdoscanrevisionas.sharepoint.com/sites/Fiskeri'},'targeting':null},{'type':'link','config':{'text':'Handel, service og detail','url':'https://bdoscanrevisionas.sharepoint.com/sites/Handelserviceogdetail'},'targeting':null},{'type':'link','config':{'text':'Healthcare/sundhed','url':'https://bdoscanrevisionas.sharepoint.com/sites/Healthcaresundhed'},'targeting':null},{'type':'link','config':{'text':'Håndværkere','url':'https://bdoscanrevisionas.sharepoint.com/sites/Haandvaerkere'},'targeting':null},{'type':'link','config':{'text':'Industri','url':'https://bdoscanrevisionas.sharepoint.com/sites/Industri2'},'targeting':null},{'type':'link','config':{'text':'Landbrug og gartneri','url':'https://bdoscanrevisionas.sharepoint.com/sites/Landbrugoggartneri'},'targeting':null},{'type':'link','config':{'text':'Oplevelse','url':'https://bdoscanrevisionas.sharepoint.com/sites/Oplevelse'},'targeting':null},{'type':'link','config':{'text':'Partnerdrevet virksomhed','url':'https://bdoscanrevisionas.sharepoint.com/sites/Partnerdrevetvirksomhed'},'targeting':null},{'type':'link','config':{'text':'Startup','url':'https://bdoscanrevisionas.sharepoint.com/sites/Startup'},'targeting':null},{'type':'link','config':{'text':'Transport','url':'https://bdoscanrevisionas.sharepoint.com/sites/Transport'},'targeting':null},{'type':'link','config':{'text':'Uddannelse','url':'https://bdoscanrevisionas.sharepoint.com/sites/Uddannelse'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Faglig info'},'elements':[{'type':'link','config':{'text':'VIDENskabet','url':'http://ph01/folder/32267/videnskabet','openInNewWindow':true},'targeting':null},{'type':'link','config':{'text':'Værktøjer'},'targeting':null},{'type':'link','config':{'text':'Revision og regnskab'},'targeting':null},{'type':'link','config':{'text':'Produkthåndbogen','url':'http://ph01/home/forside','openInNewWindow':true},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'Salg'},'elements':[{'type':'link','config':{'text':'Tilbud'},'targeting':null},{'type':'link','config':{'text':'Koncepter'},'targeting':null},{'type':'link','config':{'text':'Mit BDO'},'targeting':null},{'type':'link','config':{'text':'Brancheanalyser'},'targeting':null},{'type':'link','config':{'text':'Produktkatalog'},'targeting':null},{'type':'link','config':{'text':'BSO'},'targeting':null}],'targeting':null,'type':null}]],'targeting':{'enabled':true,'operator':'AND','department':[],'jobTitle':[],'location':[{'label':'Copenhagen','id':'99340aa6-c8e7-413a-9022-046ed872e809','field':'IATargetingLocation'}],'custom1':[],'custom2':[]},'type':'menu'},{'config':{'title':'Jeg vil...','url':'http://google.com','openInNewWindow':true,'icon':'Address'},'elementGroups':[[{'config':{'headline':'.. .godt i gang som BDO'er','openInNewWindow':false},'elements':[{'type':'link','config':{'text':'Velkomst og introduktion','url':'http://localhost:3000/#?test=test2','openInNewWindow':false},'targeting':null},{'type':'link','config':{'text':'Bestil visitkort','url':'http://localhost:3000/#?test=test'},'targeting':null},{'type':'link','config':{'text':'Medarbejderbilleder'},'targeting':null},{'type':'link','config':{'text':'Tidsregistrering'},'targeting':null},{'type':'link','config':{'text':'Politiker'},'targeting':null},{'type':'link','config':{'text':'Opsætning af it'},'targeting':null}],'targeting':null,'type':null},{'config':{'headline':'.. finde kollegaer'},'elements':[{'type':'link','config':{'text':'Telefonbog','url':'','openInNewWindow':true},'targeting':null},{'type':'link','config':{'text':'Oprette en BDO nyhed','url':'https://bdoscanrevisionas.sharepoint.com/sites/nyheder'},'targeting':null},{'type':'link','config':{'text':'Organisationsdiagram'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'.. have styr på rejser og udlæg'},'elements':[{'type':'link','config':{'text':'Afregning'},'targeting':null},{'type':'link','config':{'text':'Hotelbestilling'},'targeting':null},{'type':'link','config':{'text':'Lån af bil'},'targeting':null}],'targeting':null,'type':null},{'config':{'headline':'.. finde fagligt materiale'},'elements':[{'type':'link','config':{'text':'Produkthåndbogen'},'targeting':null},{'type':'link','config':{'text':'Videnskabet'},'targeting':null},{'type':'link','config':{'text':'Viden Om'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'.. have hjælp af it'},'elements':[{'type':'link','config':{'text':'Kontakt IT'},'targeting':null},{'type':'link','config':{'text':'Vejledninger'},'targeting':null},{'type':'link','config':{'text':'Bestille it-udstyr'},'targeting':null}],'targeting':null,'type':null},{'config':{'headline':'.. planlægge min karriere'},'elements':[{'type':'link','config':{'text':'RADAR'},'targeting':null},{'type':'link','config':{'text':'Ledige stillinger i BDO'},'targeting':null},{'type':'link','config':{'text':'Intern mobilitet'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'headline':'.. rekruttere en medarbejder'},'elements':[{'type':'link','config':{'text':'Rekruttering'},'targeting':null},{'type':'link','config':{'text':'Attract rekruteringssystem'},'targeting':null}],'targeting':null,'type':null},{'config':{'headline':'.. have information om løn'},'elements':[{'type':'link','config':{'text':'Ændring af trækprocent'},'targeting':null},{'type':'link','config':{'text':'Sådan læser du din lønseddel'},'targeting':null},{'type':'link','config':{'text':'Akkvisition'},'targeting':null},{'type':'link','config':{'text':'Rejse- og kørselsgodtgørelse'},'targeting':null}],'targeting':null,'type':null}],[{'config':{'imageUrl':'https://intraactivedev.sharepoint.com/sites/IntraActive-NavigatorAdmin/lists/IANavigatorImages/Full.png','url':'http://google.com','openInNewWindow':true},'elements':[{'type':'image','config':{'imageUrl':'https://intraactivedev.sharepoint.com/sites/IntraActive-NavigatorAdmin/lists/IANavigatorImages/Full.png'},'targeting':null}],'targeting':null,'type':'image'}]],'targeting':{'enabled':true,'operator':'AND','department':[{'label':'Tokyo','id':'8a8cc59a-821e-43c0-a0f2-ccccdee44606','field':'IATargetingDepartment'}],'jobTitle':[],'location':[],'custom1':[],'custom2':[]},'type':'menu'}]},'smartmenu':{'elements':[{'type':'menu','config':{'url':'https://www.facebook.com/','icon':'Facebook','openInNewWindow':false,'title':'Facebook'},'targeting':null},{'type':'phonebook','config':{'siteCollection':'IA-DI0913-US','title':'Phonebook','icon':'Phone'},'targeting':null}]}}))",
                });
                wait.UntilLocalStorageIsUpdated();

                await Task.Delay(5000);

                webDriver.Navigate().Refresh(); // This doesn't work
                wait.UntilUrl(url);
                webDriver.Navigate().Refresh(); // This doesn't work
                wait.UntilUrl(url);

                wait.UntilElementVisible(By.CssSelector("div[class^='subMenuItem_']:nth-child(14)"));
                webDriver.FindElementByCssSelector("div[class^='subMenuItem_']:nth-child(14)").Click();

                wait.UntilElementVisible(By.CssSelector("div[class^='IA_pivotItem_']:nth-child(3)"));
                webDriver.FindElementByCssSelector("div[class^='IA_pivotItem_']:nth-child(3)").Click();

                // We might not have to do this
                //webDriver.ExecuteScript(RemoveStyleSheetsScript);
                //webDriver.ExecuteScript(RemoveStyleTagsScript);
                //webDriver.ExecuteScript(RemoveElementStylesScript);

                return GetCurrentState();
            }
            public override async Task<bool> HasReachedAGoalCondition(State<IReadOnlyCollection<IWebElement>> state, AgentAction<IReadOnlyCollection<IWebElement>> action)
            {
                // *[data-automation-id='messages-edit-panel-save']
                // ExternalClass8514166CB2C745279663A372003C929D
                var goal = webDriver.FindElementsByCssSelector(".ExternalClass8514166CB2C745279663A372003C929D")  
                    .Where(x => x.Enabled && x.Displayed);

                return goal.Count() > 0;
            }
        }
    }

    public static class WebDriverWaitExtensions
    {
        public static void UntilLocalStorageIsUpdated(this WebDriverWait webDriverWait)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    var s = (string) ((RemoteWebDriver)x).ExecuteScript("return window.localStorage.getItem('navigator-1')");
                    return !string.IsNullOrWhiteSpace(s);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public static void UntilElementVisible(this WebDriverWait webDriverWait, By selector)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    var element = x.FindElement(selector);
                    return element.CanBeInteracted(x);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public static void UntilUrl(this WebDriverWait webDriverWait, string url)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    return x.Url == url;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }
    }
}
