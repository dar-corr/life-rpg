import { request } from "./auth";

export function createCharacter(name) {
  return request("/api/characters", {
    method: "POST",
    body: JSON.stringify({ name }),
  });
}

export function getCharacter() {
  return request("/api/characters/me");
}
