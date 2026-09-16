"""Browser regression checks against a fully built and locally served DocFX site.

Requires Playwright for Python and Google Chrome. Run from the repository root:
    python docfx/tests/navigation_smoke.py
Override DOCFX_TEST_URL to test a different local URL or deployment prefix.
"""

import os
import unittest

from playwright.sync_api import sync_playwright


BASE_URL = os.environ.get("DOCFX_TEST_URL", "http://localhost:8081/Akeldov.Math/")


class NavigationSmokeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch(channel="chrome", headless=True)

    @classmethod
    def tearDownClass(cls):
        cls.browser.close()
        cls.playwright.stop()

    def setUp(self):
        self.context = self.browser.new_context(viewport={"width": 1440, "height": 900})
        self.page = self.context.new_page()
        self.errors = []
        self.page.on("pageerror", lambda error: self.errors.append(str(error)))

    def tearDown(self):
        self.context.close()
        self.assertEqual(self.errors, [])

    def load_ready(self, path):
        self.page.goto(BASE_URL + path)
        self.page.wait_for_function("window.docfx?.ready")
        self.page.wait_for_function("!document.documentElement.classList.contains('docs-ui-pending')")

    def test_links_and_libraries_work_before_docfx_loads(self):
        self.page.route("**/public/docfx.min.js*", lambda route: route.fulfill(
            content_type="text/javascript", body=""))
        for language in ("en", "ru"):
            with self.subTest(language=language):
                self.page.goto(BASE_URL + language + "/Libraries/index.html")
                shell = self.page.locator(".docs-primary-navigation-shell")
                shell.locator("summary").click()
                library = shell.locator(".dropdown-menu a").first
                self.assertTrue(library.is_visible())
                self.assertEqual(library.get_attribute("tabindex"), None)
                self.assertTrue(library.get_attribute("href").endswith("Spatial2D/index.html"))
                shell.locator(":scope > li > a").first.click()
                self.page.wait_for_url(BASE_URL + language + "/index.html")

    def test_ready_navigation_does_not_wait_for_selectors(self):
        held_requests = []
        self.page.route("**/languages.json", lambda route: held_requests.append(route), times=1)
        self.page.goto(BASE_URL + "ru/Libraries/index.html", wait_until="domcontentloaded")
        self.page.wait_for_function("document.querySelector('#navbar > .navbar-nav:not(.docs-primary-navigation-shell)')")
        self.assertTrue(held_requests)
        self.assertEqual(self.page.locator(".docs-primary-navigation-shell").count(), 0)
        self.page.locator("#navbar > .navbar-nav > li > a").first.click()
        self.page.wait_for_url(BASE_URL + "ru/index.html", wait_until="domcontentloaded")
        for route in held_requests:
            route.abort()

    def test_open_fallback_survives_docfx_initialization(self):
        held_requests = []
        self.page.route("**/public/docfx.min.js*", lambda route: held_requests.append(route), times=1)
        self.page.goto(BASE_URL + "en/index.html", wait_until="commit")
        shell = self.page.locator(".docs-primary-navigation-shell")
        shell.locator("summary").click()
        for route in held_requests:
            route.continue_()
        self.page.wait_for_function("window.docfx?.ready")
        self.assertTrue(shell.locator(".dropdown-menu a").first.is_visible())
        self.assertEqual(self.page.locator("#navbar > .navbar-nav:visible").count(), 1)
        shell.locator(".dropdown-menu a").first.click()
        self.page.wait_for_url("**/en/Spatial2D/1.1.0/index.html")

    def test_responsive_header_and_controls(self):
        paths = (
            "en/index.html", "ru/index.html",
            "ru/Spatial2D/1.1.0/concepts/index.html",
            "en/Hexes/0.5.0/concepts/index.html",
            "api/Spatial2D/1.1.0/index.html?lang=ru",
        )
        for path in paths:
            self.load_ready(path)
            for width in (1440, 1200, 1199, 1024, 800, 768, 767, 600, 375, 320, 1440):
                with self.subTest(path=path, width=width):
                    self.page.set_viewport_size({"width": width, "height": 900})
                    if width < 768 and not self.page.locator("#navpanel").is_visible():
                        self.page.locator('[data-bs-target="#navpanel"]').click()
                        self.page.wait_for_function("document.querySelector('#navpanel').classList.contains('show')")
                    self.page.evaluate("() => new Promise(requestAnimationFrame)")
                    boxes = self.page.evaluate("""() => {
                        const box = selector => {
                            const r = document.querySelector(selector).getBoundingClientRect();
                            return {left:r.left, right:r.right, top:r.top, bottom:r.bottom, height:r.height};
                        };
                        return {
                            header:box('body > header'), main:box('body > main'),
                            nav:box('#navbar > .navbar-nav'), icons:box('#navbar form.icons'),
                            search:box('#search'), repo:box('#akeldov-repository-link'),
                            language:box('#akeldov-docs-language'),
                            theme:box('#navbar form.icons > .dropdown:last-child'),
                            measuredHeight:parseFloat(document.body.style.getPropertyValue('--docs-header-height'))
                        };
                    }""")
                    for name in ("nav", "icons", "search", "repo"):
                        box = boxes[name]
                        self.assertGreater(box["height"], 0, name)
                        self.assertGreaterEqual(box["left"], boxes["header"]["left"] - 1, name)
                        self.assertLessEqual(box["right"], boxes["header"]["right"] + 1, name)
                        self.assertLessEqual(box["bottom"], boxes["header"]["bottom"] + 1, name)
                    self.assertGreaterEqual(boxes["main"]["top"], boxes["header"]["bottom"] - 1)
                    self.assertAlmostEqual(boxes["measuredHeight"], boxes["header"]["height"], delta=1)
                    self.assertLessEqual(boxes["language"]["right"], boxes["theme"]["left"] + 1)
                    self.assertLessEqual(boxes["icons"]["right"], boxes["search"]["left"] + 1)
                    self.assertLessEqual(boxes["search"]["right"], boxes["repo"]["left"] + 1)
                    self.assertAlmostEqual(
                        boxes["search"]["top"] + boxes["search"]["height"] / 2,
                        boxes["repo"]["top"] + boxes["repo"]["height"] / 2, delta=1)

    def test_theme_language_and_search_remain_usable(self):
        self.load_ready("en/index.html")
        self.page.locator('#navbar form.icons > .dropdown:last-child > a').click()
        self.page.get_by_text("Dark", exact=True).click()
        self.assertEqual(self.page.locator("html").get_attribute("data-bs-theme"), "dark")
        self.page.locator("#akeldov-docs-language").click()
        self.page.locator('#akeldov-docs-language-container a[hreflang="ru"]').click()
        self.page.wait_for_url(BASE_URL + "ru/index.html")
        self.page.locator("#search-query").fill("Voronoi")
        self.page.wait_for_function("document.querySelectorAll('#search-results a').length > 0")

    def test_version_and_language_switches_preserve_section(self):
        for library, current, target in (("Spatial2D", "1.1.0", "0.9.0"), ("Hexes", "0.5.0", "0.2.0")):
            with self.subTest(library=library):
                self.load_ready(f"ru/{library}/{current}/concepts/index.html")
                self.page.locator("#akeldov-docs-version").select_option(target)
                self.page.wait_for_url(BASE_URL + f"ru/{library}/{target}/concepts/index.html")
                self.page.locator("#akeldov-docs-language").click()
                self.page.locator('#akeldov-docs-language-container a[hreflang="en"]').click()
                self.page.wait_for_url(BASE_URL + f"en/{library}/{target}/concepts/index.html")
                self.page.locator(".docs-context-link").last.click()
                self.page.wait_for_url(BASE_URL + f"api/{library}/{target}/index.html")
                self.page.wait_for_function("!document.documentElement.classList.contains('docs-toc-pending')")
                other_library = "Hexes" if library == "Spatial2D" else "Spatial2D"
                self.assertNotIn(f"Akeldov.Math.{other_library}", self.page.locator("#toc").inner_text())

    def header_snapshot(self):
        return self.page.evaluate("""() => {
          window.__docsHeaderSnapshot = () => {
            const visible = selector => [...document.querySelectorAll(selector)].find(e =>
                e.getBoundingClientRect().height && getComputedStyle(e).visibility !== 'hidden');
            const primary = visible('#navbar > .navbar-nav');
            const icons = visible('.docs-icons-placeholder, #navbar form.icons');
            const context = visible('.docs-context-navigation');
            const elements = {
                header:visible('body > header'), brand:visible('.navbar-brand'),
                menuToggle:visible('[data-bs-target="#navpanel"]'),
                primary, icons, search:visible('#search'),
                language:icons?.querySelector('.docs-language-button'),
                theme:icons?.querySelector('.dropdown:last-child > a'),
                repository:visible('.docs-repository-link'),
                context, version:visible('.docs-version-selector select')
            };
            return {
                boxes:Object.fromEntries(Object.entries(elements).map(([key, e]) => {
                    const r = e?.getBoundingClientRect();
                    return [key, r ? {x:r.x,y:r.y,width:r.width,height:r.height} : null];
                })),
                colorTheme:document.documentElement.dataset.bsTheme,
                primaryLabels:primary ? [...primary.querySelectorAll(':scope > li > a, summary')].map(e => e.textContent.trim()) : [],
                contextLabels:context ? [...context.querySelectorAll('.docs-context-link')].map(e => e.textContent.trim()) : [],
                versionLabel:elements.version?.selectedOptions[0]?.textContent ?? null,
                themeIcon:[...(elements.theme?.querySelector('i')?.classList ?? [])].find(c => c.startsWith('bi-')) ?? null
            };
          };
          return window.__docsHeaderSnapshot();
        }""")

    def assert_header_stable(self, before, after):
        for key in ("primaryLabels", "contextLabels", "versionLabel", "themeIcon", "colorTheme"):
            self.assertEqual(before[key], after[key], key)
        for name, box in before["boxes"].items():
            with self.subTest(control=name):
                if box is None:
                    self.assertIsNone(after["boxes"][name])
                else:
                    self.assertIsNotNone(after["boxes"][name])
                    for dimension, value in box.items():
                        self.assertAlmostEqual(value, after["boxes"][name][dimension], delta=1,
                                               msg=f"{name}.{dimension}")

    def test_controls_do_not_shift_during_page_transitions(self):
        paths = ("ru/index.html", "ru/Libraries/index.html",
                 "ru/Spatial2D/1.1.0/concepts/index.html",
                 "ru/Spatial2D/1.1.0/tutorials/index.html",
                 "api/Spatial2D/1.1.0/index.html?lang=ru",
                 "api/Hexes/0.5.0/index.html",
                 "ru/Spatial2D/0.8.0/concepts/index.html",
                 "en/Hexes/upcoming/concepts/index.html")
        self.context.add_init_script("localStorage.setItem('theme', 'dark')")
        for width in (1440, 1024, 375):
            self.page.set_viewport_size({"width": width, "height": 900})
            for path in paths:
                with self.subTest(path=path, width=width):
                    held_requests = []
                    self.page.route("**/public/docfx.min.js*", lambda route: held_requests.append(route), times=1)
                    self.page.goto(BASE_URL + path, wait_until="commit")
                    self.page.locator(".docs-primary-navigation-shell").wait_for(state="attached")
                    self.page.evaluate("() => document.fonts.ready")
                    before = self.header_snapshot()
                    self.page.evaluate("""() => {
                        window.__docsHeaderFrames = [];
                        window.__docsCaptureFrames = true;
                        const capture = () => {
                            if (!window.__docsCaptureFrames) return;
                            window.__docsHeaderFrames.push(window.__docsHeaderSnapshot());
                            requestAnimationFrame(capture);
                        };
                        requestAnimationFrame(capture);
                    }""")
                    for route in held_requests:
                        route.continue_()
                    self.page.wait_for_function("window.docfx?.ready")
                    self.page.wait_for_function("!document.documentElement.classList.contains('docs-ui-pending')")
                    if before["versionLabel"] is not None:
                        self.page.locator("#akeldov-docs-version").wait_for()
                    self.page.evaluate("() => new Promise(requestAnimationFrame)")
                    self.assert_header_stable(before, self.header_snapshot())
                    frames = self.page.evaluate("() => { window.__docsCaptureFrames = false; return window.__docsHeaderFrames; }")
                    for frame in frames:
                        self.assert_header_stable(before, frame)

    def test_slow_selectors_do_not_remove_visible_controls(self):
        held_requests = []
        self.page.route("**/languages.json", lambda route: held_requests.append(route), times=1)
        self.page.goto(BASE_URL + "ru/index.html")
        self.page.locator('#navbar form.icons').wait_for(state="attached")
        before = self.header_snapshot()
        # Cross the old three-second fallback timer with the language request pending.
        self.page.wait_for_timeout(3200)
        self.assert_header_stable(before, self.header_snapshot())
        for route in held_requests:
            route.continue_()
        self.page.wait_for_function("!document.documentElement.classList.contains('docs-ui-pending')")
        self.assert_header_stable(before, self.header_snapshot())


if __name__ == "__main__":
    unittest.main(verbosity=2)
