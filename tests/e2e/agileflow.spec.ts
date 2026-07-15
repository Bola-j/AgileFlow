import { expect, test, type APIRequestContext, type Page } from "@playwright/test";

const apiBaseUrl = process.env.PLAYWRIGHT_API_URL ?? "http://127.0.0.1:6358";
const password = "Admin123!";

test("registers, signs in, and loads dashboard", async ({ page }) => {
  const email = `e2e-${Date.now()}@example.com`;
  await register(page, email);
  await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();
});

test("@editing user profile edit persists", async ({ page, request }) => {
  const email = `editing-${Date.now()}@example.com`;
  const auth = await registerViaApi(request, email);
  await page.addInitScript((storedAuth) => {
    localStorage.setItem("agileflow.auth", JSON.stringify(storedAuth));
  }, { ...auth, remember: true });

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

async function registerViaApi(request: APIRequestContext, email: string) {
  const response = await request.post(`${apiBaseUrl}/api/auth/register`, {
    data: { firstName: "E2E", lastName: "Editor", email, password },
  });
  expect(response.ok()).toBeTruthy();
  return response.json();
}
