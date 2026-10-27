import { expect, Locator, Page } from '@playwright/test';

export const DESKTOP = { width: 1385, height: 900 };
export const MOBILE = { width: 600, height: 900 };

/** Section grid for the nth <h2> on /people/ (page order, see SECTION_INDEX) */
export function sectionByIndex(page: Page, index: number): Locator {
  return page.locator('h2').nth(index).locator('xpath=following-sibling::div[contains(@class,"employee-grid")][1]');
}

export const SECTION_INDEX = {
  plain: 0,
  pad: 1,
  eager: 2,
  webp: 3,
  localCrop: 4,
  globalCrop: 5,
  boxPad: 6,
  service: 7,
  urlHelper: 8,
  urlHelperGlobal: 9,
  urlHelperLocal: 10,
  localCrops: 11,
  mobileCrop: 12,
} as const;

/** Scroll the whole page slowly so lazysizes unveils everything, then wait for network idle */
export async function unveilAll(page: Page) {
  await page.evaluate(async () => {
    const step = Math.max(200, window.innerHeight / 2);
    for (let y = 0; y < document.body.scrollHeight; y += step) {
      window.scrollTo(0, y);
      await new Promise((r) => setTimeout(r, 60));
    }
    window.scrollTo(0, 0);
  });
  await page.waitForFunction(
    () => [...document.images].every((i) => !i.classList.contains('lazyload') && !i.classList.contains('lazyloading')),
    undefined,
    { timeout: 30_000 },
  );
  await page.waitForLoadState('networkidle');
}

export async function expectImagesLoaded(images: Locator) {
  const count = await images.count();
  expect(count).toBeGreaterThan(0);
  for (let i = 0; i < count; i++) {
    const img = images.nth(i);
    await img.scrollIntoViewIfNeeded();
    await expect
      .poll(() => img.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0), {
        message: `image #${i} should load`,
      })
      .toBe(true);
  }
}

export interface SourceInfo {
  type: string | null;
  media: string | null;
  sizes: string | null;
  dataSizes: string | null;
  srcset: string | null;
  dataSrcset: string | null;
  width: string | null;
  height: string | null;
}

export async function sourcesOf(picture: Locator): Promise<SourceInfo[]> {
  return picture.locator('source').evaluateAll((els) =>
    els.map((s) => ({
      type: s.getAttribute('type'),
      media: s.getAttribute('media'),
      sizes: s.getAttribute('sizes'),
      dataSizes: s.getAttribute('data-sizes'),
      srcset: s.getAttribute('srcset'),
      dataSrcset: s.getAttribute('data-srcset'),
      width: s.getAttribute('width'),
      height: s.getAttribute('height'),
    })),
  );
}

/** Parse the width descriptors ("180w") from a srcset string */
export function descriptors(srcset: string | null): number[] {
  return [...(srcset ?? '').matchAll(/\s(\d+)w(?:,|$)/g)].map((m) => Number(m[1]));
}

export function widthParam(url: string): number | undefined {
  const m = /[?&]width=(\d+)/.exec(url);
  return m ? Number(m[1]) : undefined;
}

/** Collect failed image requests (status >= 400 or requestfailed) during a test */
export function trackFailedImages(page: Page): string[] {
  const failed: string[] = [];
  page.on('requestfailed', (r) => {
    if (r.resourceType() === 'image') failed.push(`FAILED ${r.url()}`);
  });
  page.on('response', (r) => {
    if (r.request().resourceType() === 'image' && r.status() >= 400) failed.push(`${r.status()} ${r.url()}`);
  });
  return failed;
}
