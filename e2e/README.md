# Slimsy e2e tests

Playwright tests that drive the demo site (`src/Slimsy.TestSite`) `/people/` page.

## Run

```bash
cd e2e
npm install
npx playwright install chromium
npm test            # starts the demo site automatically (dotnet run on http://localhost:57496)
npm run report      # open the HTML report
```

If the demo site is already running on port 57496 it is reused.

## What is covered

One spec, `tests/people.spec.ts`, covers all 13 sections of `/people/` (sections are located by `<h2>` order, see `SECTION_INDEX` in `tests/helpers.ts` - add new demo sections at the end or update the indexes):

- plain / UrlHelper / LocalCrops sections: images load, no `<picture>`
- `SlimsyService` srcset section: lazysizes unveil and measured `sizes`
- every `<slimsy-picture>` section: source order (webp, avif, native), single-source gifs, width/height attributes, images load, no failed requests
- lazy markup (JS disabled) vs unveiled markup, eager markup, `fetchpriority`, decorative (`alt=""`, `role=presentation`)
- crop modes (pad, boxpad), local/global `crop-alias`, explicit width/height preserved
- `mobile-crop-alias`: media queries, per-source dimensions, crop switched by viewport (1385px vs 600px) and on resize

## Not covered by the demo page yet

The demo page has no examples of these, so they are only covered by unit tests:

- lazy `manual-sizes` (should render real `sizes` on `<source>`)
- `crop-alias` without explicit width/height (should use the crop's dimensions)
