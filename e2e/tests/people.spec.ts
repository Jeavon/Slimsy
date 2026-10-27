import { expect, test } from '@playwright/test';
import {
  DESKTOP,
  MOBILE,
  SECTION_INDEX as S,
  descriptors,
  expectImagesLoaded,
  sectionByIndex,
  sourcesOf,
  trackFailedImages,
  unveilAll,
  widthParam,
} from './helpers';

test.use({ viewport: DESKTOP });

test.beforeEach(async ({ page }) => {
  await page.goto('/people/');
});

const currentSrc = (img: import('@playwright/test').Locator) => img.evaluate((el: HTMLImageElement) => el.currentSrc);

test.describe('page structure', () => {
  test('renders every demo section', async ({ page }) => {
    await expect(page.locator('h2')).toHaveCount(13);
    for (let i = 0; i < 13; i++) {
      await expect(sectionByIndex(page, i)).toHaveCount(1);
      expect(await sectionByIndex(page, i).locator('.employee-grid__item').count()).toBeGreaterThan(0);
    }
  });

  test('all images on the page load without failed requests', async ({ page }) => {
    const failed = trackFailedImages(page);
    await page.goto('/people/');
    await unveilAll(page);
    const broken = await page.evaluate(() =>
      [...document.images].filter((i) => !(i.complete && i.naturalWidth > 0)).map((i) => i.currentSrc || i.src),
    );
    expect(broken).toEqual([]);
    expect(failed).toEqual([]);
  });
});

test.describe('sections without Slimsy markup', () => {
  for (const [name, idx] of [
    ['plain', S.plain],
    ['urlHelper', S.urlHelper],
    ['urlHelperGlobal', S.urlHelperGlobal],
    ['urlHelperLocal', S.urlHelperLocal],
    ['localCrops', S.localCrops],
  ] as const) {
    test(`${name}: images load and are not wrapped in <picture>`, async ({ page }) => {
      const grid = sectionByIndex(page, idx);
      await expect(grid.locator('picture')).toHaveCount(0);
      await expectImagesLoaded(grid.locator('img'));
    });
  }

  test('localCrops: each person has 7 captioned variants', async ({ page }) => {
    const first = sectionByIndex(page, S.localCrops).locator('.employee-grid__item').first();
    await expect(first.locator('figure')).toHaveCount(7);
    await expect(first.locator('figcaption').first()).toHaveText('LocalCrop UrlHelper');
  });
});

test.describe('SlimsyService srcset section', () => {
  test('img with data-srcset is unveiled by lazysizes with measured sizes', async ({ page }) => {
    const grid = sectionByIndex(page, S.service);
    const img = grid.locator('img').first();
    await img.scrollIntoViewIfNeeded();
    await expect(img).toHaveClass(/lazyloaded/);
    expect(descriptors(await img.getAttribute('srcset')).length).toBeGreaterThan(1);
    expect(await img.getAttribute('sizes')).toMatch(/^\d+px$/);
    await expectImagesLoaded(grid.locator('img'));
  });
});

/** The <picture> sections rendered by <slimsy-picture> */
const PICTURE_SECTIONS = [
  ['pad', S.pad],
  ['eager (first) + lazy', S.eager],
  ['webp', S.webp],
  ['local crop', S.localCrop],
  ['global crop', S.globalCrop],
  ['boxPad', S.boxPad],
  ['mobile crop', S.mobileCrop],
] as const;

for (const [name, idx] of PICTURE_SECTIONS) {
  test.describe(`slimsy-picture: ${name}`, () => {
    test('every item renders a <picture> with webp, avif and native sources in order', async ({ page }) => {
      const pictures = sectionByIndex(page, idx).locator('picture');
      const count = await pictures.count();
      expect(count).toBeGreaterThan(0);
      for (let i = 0; i < count; i++) {
        const types = (await sourcesOf(pictures.nth(i))).map((s) => s.type);
        if (types.length === 1) {
          // SingleSources (gif) renders a single native source
          expect(types).toEqual(['image/gif']);
          continue;
        }
        // mobile-crop renders the mobile set first, then the desktop set
        const desktopTypes = name === 'mobile crop' ? types.slice(types.length / 2) : types;
        expect(desktopTypes.slice(0, 2)).toEqual(['image/webp', 'image/avif']);
        expect(desktopTypes[2]).toMatch(/^image\/(jpeg|png)$/);
        await expect(pictures.nth(i).locator('img')).toHaveCount(1);
      }
    });

    test('all images load without failed requests', async ({ page }) => {
      const failed = trackFailedImages(page);
      await expectImagesLoaded(sectionByIndex(page, idx).locator('picture img'));
      expect(failed).toEqual([]);
    });

    test('rendered <img> has non-zero width and height attributes (ImageDimensions on)', async ({ page }) => {
      const imgs = sectionByIndex(page, idx).locator('picture img');
      const count = await imgs.count();
      for (let i = 0; i < count; i++) {
        expect(Number(await imgs.nth(i).getAttribute('width')), `img #${i} width`).toBeGreaterThan(0);
        expect(Number(await imgs.nth(i).getAttribute('height')), `img #${i} height`).toBeGreaterThan(0);
      }
    });
  });
}

test.describe('lazy loading (default)', () => {
  test('server markup: lazyload class, data-srcset, auto data-sizes, LQIP placeholder', async ({ browser }) => {
    // JS disabled so lazysizes can't mutate the server markup
    const ctx = await browser.newContext({ javaScriptEnabled: false, viewport: DESKTOP });
    const page = await ctx.newPage();
    await page.goto('/people/');
    const picture = sectionByIndex(page, S.webp).locator('picture').first();
    for (const s of await sourcesOf(picture)) {
      expect(descriptors(s.dataSrcset).length).toBeGreaterThan(3);
      expect(s.dataSizes).toBe('auto');
      expect(s.sizes).toBeNull();
      expect(s.srcset).toMatch(/quality=20/);
      expect(s.srcset).not.toMatch(/\d+w/);
    }
    const img = picture.locator('img');
    await expect(img).toHaveClass(/lazyload/);
    await expect(img).toHaveAttribute('data-sizes', 'auto');
    await expect(img).toHaveAttribute('data-src', /.+/);
    await expect(img).toHaveAttribute('src', /quality=20/);
    await ctx.close();
  });

  test('after unveil lazysizes sets measured sizes on <img> and every <source>, plus srcset', async ({ page }) => {
    const picture = sectionByIndex(page, S.webp).locator('picture').first();
    await picture.scrollIntoViewIfNeeded();
    await expect(picture.locator('img')).toHaveClass(/lazyloaded/);
    for (const s of await sourcesOf(picture)) {
      expect(s.sizes).toMatch(/^\d+px$/);
      expect(descriptors(s.srcset).length).toBeGreaterThan(3);
    }
  });
});

test.describe('eager loading + fetch priority', () => {
  test('first item is eager: real srcset/sizes in markup, no lazyload, fetchpriority=high, no LQIP', async ({ browser }) => {
    const ctx = await browser.newContext({ javaScriptEnabled: false, viewport: DESKTOP });
    const page = await ctx.newPage();
    await page.goto('/people/');
    const picture = sectionByIndex(page, S.eager).locator('picture').first();
    const img = picture.locator('img');
    await expect(img).not.toHaveClass(/lazyload/);
    await expect(img).toHaveAttribute('fetchpriority', 'high');
    await expect(img).toHaveAttribute('src', /.+/);
    await expect(img).not.toHaveAttribute('data-src', /.+/);
    await expect(img).toHaveAttribute('sizes', '100vw');
    for (const s of await sourcesOf(picture)) {
      expect(descriptors(s.srcset).length).toBeGreaterThan(3);
      expect(s.sizes).toBe('100vw');
      expect(s.dataSrcset).toBeNull();
      expect(s.dataSizes).toBeNull();
    }
    await ctx.close();
  });

  test('other items in the eager section stay lazy', async ({ page }) => {
    const second = sectionByIndex(page, S.eager).locator('picture').nth(1).locator('img');
    await expect(second).toHaveClass(/lazyload/);
    await expect(second).not.toHaveAttribute('fetchpriority', /.+/);
  });

  test('pad section: only the first item has fetchpriority=high', async ({ page }) => {
    const imgs = sectionByIndex(page, S.pad).locator('picture img');
    await expect(imgs.first()).toHaveAttribute('fetchpriority', 'high');
    const count = await imgs.count();
    for (let i = 1; i < count; i++) {
      await expect(imgs.nth(i)).not.toHaveAttribute('fetchpriority', /.+/);
    }
  });
});

test.describe('decorative images', () => {
  for (const [name, idx] of [
    ['pad', S.pad],
    ['eager', S.eager],
  ] as const) {
    test(`${name}: decorative images have empty alt and role=presentation`, async ({ page }) => {
      const imgs = sectionByIndex(page, idx).locator('picture img');
      const count = await imgs.count();
      for (let i = 0; i < count; i++) {
        await expect(imgs.nth(i)).toHaveAttribute('alt', '');
        await expect(imgs.nth(i)).toHaveAttribute('role', 'presentation');
      }
    });
  }

  test('non-decorative section: alt text is set, no role', async ({ page }) => {
    const imgs = sectionByIndex(page, S.webp).locator('picture img');
    const count = await imgs.count();
    for (let i = 0; i < count; i++) {
      expect(((await imgs.nth(i).getAttribute('alt')) ?? '').length).toBeGreaterThan(0);
      await expect(imgs.nth(i)).not.toHaveAttribute('role', /.+/);
    }
  });
});

test.describe('crop modes and crop aliases', () => {
  test('explicit width/height alongside crop-alias are preserved on <img>', async ({ page }) => {
    for (const idx of [S.localCrop, S.globalCrop]) {
      const img = sectionByIndex(page, idx).locator('picture img').first();
      await expect(img).toHaveAttribute('width', '323');
      await expect(img).toHaveAttribute('height', '300');
    }
  });

  test('local crop srcset urls carry crop coordinates (cc=) and square ratios', async ({ page }) => {
    const picture = sectionByIndex(page, S.localCrop).locator('picture').first();
    await picture.scrollIntoViewIfNeeded();
    await expect(picture.locator('img')).toHaveClass(/lazyloaded/);
    const src = (await picture.locator('source').first().getAttribute('srcset')) ?? '';
    expect(src).toMatch(/cc=/);
    for (const u of src.split(/,(?=\/)/)) {
      expect(/width=(\d+)/.exec(u)?.[1]).toBe(/height=(\d+)/.exec(u)?.[1]);
    }
  });

  test('pad and boxpad sections request the matching rmode', async ({ page }) => {
    for (const [idx, mode] of [
      [S.pad, 'pad'],
      [S.boxPad, 'boxpad'],
    ] as const) {
      const picture = sectionByIndex(page, idx).locator('picture').first();
      const set = (await picture.locator('source').first().getAttribute('data-srcset')) ?? '';
      expect(set.toLowerCase()).toContain(`rmode=${mode}`);
    }
  });
});

test.describe('mobile crop (mobile-crop-alias)', () => {
  test('markup: mobile sources have a max-width media query and their own crop dimensions', async ({ page }) => {
    const picture = sectionByIndex(page, S.mobileCrop).locator('picture').first();
    const sources = await sourcesOf(picture);
    const mobile = sources.filter((s) => s.media);
    const desktop = sources.filter((s) => !s.media);
    expect(mobile).toHaveLength(3);
    expect(desktop).toHaveLength(3);
    const set = (s: (typeof sources)[number]) => s.dataSrcset ?? s.srcset;
    for (const s of mobile) {
      expect(s.media).toBe('(max-width: 720px)');
      expect(Math.max(...descriptors(set(s)))).toBeLessThanOrEqual(720);
      expect(Number(s.width)).toBeGreaterThan(0);
      expect(Number(s.height)).toBeGreaterThan(0);
    }
    for (const s of desktop) {
      expect(Math.min(...descriptors(set(s)))).toBeGreaterThan(720);
      expect(Number(s.width)).toBeGreaterThan(0);
      expect(Number(s.height)).toBeGreaterThan(0);
    }
    const cc = (u: string | null) => /cc=([^&]+)/.exec(u ?? '')?.[1];
    expect(cc(set(mobile[0]))).not.toBe(cc(set(desktop[0])));
  });

  test.describe('desktop viewport', () => {
    test.use({ viewport: DESKTOP });
    test('picks a desktop-crop candidate (> 720w)', async ({ page }) => {
      const picture = sectionByIndex(page, S.mobileCrop).locator('picture').first();
      await picture.scrollIntoViewIfNeeded();
      const img = picture.locator('img');
      await expect(img).toHaveClass(/lazyloaded/);
      expect(widthParam(await currentSrc(img))).toBeGreaterThan(720);
    });
  });

  test.describe('mobile viewport', () => {
    test.use({ viewport: MOBILE });
    test('picks a mobile-crop candidate (<= 720w)', async ({ page }) => {
      const picture = sectionByIndex(page, S.mobileCrop).locator('picture').first();
      await picture.scrollIntoViewIfNeeded();
      const img = picture.locator('img');
      await expect(img).toHaveClass(/lazyloaded/);
      await expect.poll(() => currentSrc(img)).toMatch(/[?&]width=\d+/);
      const current = await currentSrc(img);
      expect(widthParam(current)).toBeLessThanOrEqual(720);
      expect(current).toContain('cc=');
      await expectImagesLoaded(sectionByIndex(page, S.mobileCrop).locator('picture img'));
    });
  });

  test('resizing across the breakpoint switches crop', async ({ page }) => {
    const picture = sectionByIndex(page, S.mobileCrop).locator('picture').first();
    await picture.scrollIntoViewIfNeeded();
    const img = picture.locator('img');
    await expect(img).toHaveClass(/lazyloaded/);
    const desktopSrc = await currentSrc(img);
    await page.setViewportSize(MOBILE);
    await expect.poll(async () => widthParam(await currentSrc(img)) ?? Infinity).toBeLessThanOrEqual(720);
    expect(await currentSrc(img)).not.toBe(desktopSrc);
  });
});

test.describe('single source', () => {
  test('non-single-source (jpg) media still render multiple sources', async ({ page }) => {
    const sources = await sourcesOf(sectionByIndex(page, S.webp).locator('picture').first());
    expect(sources.length).toBeGreaterThanOrEqual(3);
  });
});
