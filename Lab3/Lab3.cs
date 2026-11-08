using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Lab3
{
    
    public class Variant3Tests
    {
        private WebDriver driver;
        private WebDriverWait wait;
        private const string BaseUrl = "https://the-internet.herokuapp.com";

        [SetUp]
        public void SetUp()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
                driver.Quit();
                driver.Dispose();
        }

        [Test]
        public void Test1_Checkboxes()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/checkboxes");
            var boxes = driver.FindElements(By.CssSelector("#checkboxes input[type='checkbox']"));

            Assert.That(boxes.Count, Is.EqualTo(2));
            Assert.That(boxes[0].Selected, Is.False);
            Assert.That(boxes[1].Selected, Is.True);

            boxes[0].Click();
            boxes[1].Click();

            Thread.Sleep(1000);

            Assert.That(boxes[0].Selected, Is.True);
            Assert.That(boxes[1].Selected, Is.False);
        }

        [Test]
        public void Test2_AddRemoveElements()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/add_remove_elements/");
            var addButton = driver.FindElement(By.XPath("//button[text()='Add Element']"));

            for (int i = 0; i < 3; i++)
            {
                addButton.Click();
            }

            Thread.Sleep(1000);

            var deleteButtons = driver.FindElements(By.CssSelector(".added-manually"));
            Assert.That(deleteButtons.Count, Is.EqualTo(3));

            deleteButtons[0].Click();
            Assert.That(driver.FindElements(By.CssSelector(".added-manually")).Count, Is.EqualTo(2));

            foreach (var b in driver.FindElements(By.CssSelector(".added-manually")).ToList())
            {
                b.Click();
            }

            Assert.That(driver.FindElements(By.CssSelector(".added-manually")), Is.Empty);
        }

        [Test]
        public void Test3_Dropdown()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/dropdown");
            var select = new SelectElement(driver.FindElement(By.Id("dropdown")));

            Assert.That(select.Options.Count, Is.EqualTo(3));
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Please select an option"));

            select.SelectByText("Option 1");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 1"));

            Thread.Sleep(1000);

            select.SelectByValue("2");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 2"));
        }

        [Test]
        public void Test4_Inputs()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/inputs");
            var input = driver.FindElement(By.CssSelector("input[type='number']"));

            input.SendKeys("123");
            Assert.That(input.GetAttribute("value"), Is.EqualTo("123"));

            Thread.Sleep(1000);

            input.SendKeys(Keys.ArrowUp);
            Assert.That(input.GetAttribute("value"), Is.EqualTo("124"));

            input.SendKeys(Keys.ArrowDown);
            input.SendKeys(Keys.ArrowDown);
            Assert.That(input.GetAttribute("value"), Is.EqualTo("122"));

            input.Clear();
            input.SendKeys("abc");
            Assert.That(input.GetAttribute("value"), Is.Empty);
        }

        [TestCase("200")]
        [TestCase("301")]
        [TestCase("404")]
        [TestCase("500")]
        public void Test5_StatusCodes(string code)
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/status_codes");
            driver.FindElement(By.LinkText(code)).Click();

            Assert.That(driver.Url, Does.Contain($"/status_codes/{code}"));
            var text = driver.FindElement(By.CssSelector(".example p")).Text;
            Assert.That(text, Does.Contain($"This page returned a {code} status code"));

            Thread.Sleep(1000);

            driver.FindElement(By.LinkText("here")).Click();
            Assert.That(driver.Url, Does.EndWith("/status_codes"));
        }

        [Test]
        public void Test6_NestedFrames()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/nested_frames");

            driver.SwitchTo().Frame("frame-top");
            driver.SwitchTo().Frame("frame-left");
            Assert.That(driver.FindElement(By.TagName("body")).Text.Trim(), Is.EqualTo("LEFT"));

            driver.SwitchTo().ParentFrame();    
            driver.SwitchTo().Frame("frame-middle");
            Assert.That(driver.FindElement(By.Id("content")).Text.Trim(), Is.EqualTo("MIDDLE"));

            driver.SwitchTo().ParentFrame();
            driver.SwitchTo().Frame("frame-right");
            Assert.That(driver.FindElement(By.TagName("body")).Text.Trim(), Is.EqualTo("RIGHT"));

            driver.SwitchTo().DefaultContent();
            driver.SwitchTo().Frame("frame-bottom");
            Assert.That(driver.FindElement(By.TagName("body")).Text.Trim(), Is.EqualTo("BOTTOM"));
        }

        [Test]
        public void Test7_FormAuthentication_ValidLoginAndLogout()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/login");

            driver.FindElement(By.Id("username")).SendKeys("tomsmith");
            driver.FindElement(By.Id("password")).SendKeys("SuperSecretPassword!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            Assert.That(driver.Url, Does.Contain("/secure"));
            Assert.That(driver.FindElement(By.Id("flash")).Text, Does.Contain("You logged into a secure area!"));

            driver.FindElement(By.CssSelector("a.button.secondary")).Click();
            Assert.That(driver.Url, Does.Contain("/login"));
            Assert.That(driver.FindElement(By.Id("flash")).Text, Does.Contain("You logged out of the secure area!"));
        }

        [Test]
        public void Test7_FormAuthentication_InvalidCredentials()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/login");

            driver.FindElement(By.Id("username")).SendKeys("user");
            driver.FindElement(By.Id("password")).SendKeys("123");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            Thread.Sleep(1000);

            Assert.That(driver.FindElement(By.Id("flash")).Text, Does.Contain("Your username is invalid!"));
            Assert.That(driver.Url, Does.Contain("/login"));
        }

        [Test]
        public void Test8_DynamicContent_ChangesAfterRefresh()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/dynamic_content");
            string Snapshot() => string.Join("|", driver.FindElements(By.CssSelector("#content .large-10")).Select(e => e.Text));

            string before = Snapshot();
            Assert.That(driver.FindElements(By.CssSelector("#content .row")).Count, Is.GreaterThanOrEqualTo(3));

            bool changed = false;
            for (int i = 0; i < 5 && !changed; i++)
            {
                driver.Navigate().Refresh();
                changed = Snapshot() != before;
            }

            Assert.That(changed, Is.True);
        }

        [Test]
        public void Test9_DisappearingElements()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/disappearing_elements");
            var constant = new[] { "Home", "About", "Contact Us", "Portfolio" };

            var counts = new HashSet<int>();

            for (int i = 0; i < 5; i++)
            {
                var items = driver.FindElements(By.CssSelector("ul li a")).Select(a => a.Text).ToList();
                foreach (var name in constant)
                {
                    Assert.That(items, Does.Contain(name));
                }

                Assert.That(items.Count, Is.InRange(4, 5));
                counts.Add(items.Count);
                driver.Navigate().Refresh();
            }
            TestContext.Out.WriteLine("Observed menu item counts:: " + string.Join(", ", counts));
        }


        [Test]
        public void Test10_Hover()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/hovers");
            var figures = driver.FindElements(By.CssSelector(".figure"));

            Assert.That(figures.Count, Is.EqualTo(3));

            var actions = new Actions(driver);

            for (int i = 0; i < figures.Count; i++)
            {
                Assert.That(figures[i].FindElement(By.CssSelector(".figcaption")).Displayed, Is.False);

                actions.MoveToElement(figures[i]).Perform();

                var caption = figures[i].FindElement(By.CssSelector(".figcaption"));

                Assert.That(caption.Displayed, Is.True);
                Assert.That(caption.FindElement(By.TagName("h5")).Text, Is.EqualTo($"name: user{i + 1}"));
                Assert.That(caption.FindElement(By.TagName("a")).GetAttribute("href"), Does.EndWith($"/users/{i + 1}"));
            }
        }
    }
}