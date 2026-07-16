import { expect, test, type Page } from "@playwright/test";

const password = "Admin123!";
const confirmedEmail = process.env.PLAYWRIGHT_CONFIRMED_EMAIL;
const confirmedPassword = process.env.PLAYWRIGHT_CONFIRMED_PASSWORD ?? password;

test("registers and shows email verification state", async ({ page }) => {
  const email = `e2e-${Date.now()}@example.com`;
  await register(page, email);
  await expect(page.getByText(`We sent a verification link to ${email}.`)).toBeVisible();
});

test("@editing user profile edit persists", async ({ page }) => {
  test.skip(!confirmedEmail, "Set PLAYWRIGHT_CONFIRMED_EMAIL for authenticated editing coverage.");

  await page.goto("/login");
  await page.getByLabel("Email").fill(confirmedEmail!);
  await page.locator('input[name="password"]').fill(confirmedPassword);
  await page.getByRole("button", { name: "Sign in" }).click();
  await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();

  await page.goto("/account");
  await page.getByLabel("First name").fill("Edited");
  await page.getByLabel("Last name").fill("User");
  await page.getByRole("button", { name: "Save profile" }).click();
  await expect(page.getByText("Profile updated.")).toBeVisible();
  await page.reload();
  await expect(page.getByLabel("First name")).toHaveValue("Edited");
});

async function register(page: Page, email: string) {
  await page.goto("/register");
  await page.getByLabel("First name").fill("E2E");
  await page.getByLabel("Last name").fill("User");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill(password);
  await page.getByRole("button", { name: "Create account" }).click();
}
