const API_URL = import.meta.env.VITE_API_URL;

export async function request(url, options = {}) {
  const response = await fetch(`${API_URL}${url}`, {
    ...options,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const error = new Error(
      data?.message || data?.errors?.join?.("\n") || "Ошибка запроса",
    );

    error.status = response.status;
    throw error;
  }

  return data;
}

export function register(username, email, password) {
  return request("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({
      username,
      email,
      password,
    }),
  });
}

export function login(login, password, rememberMe = false) {
  return request("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({
      login,
      password,
      rememberMe,
    }),
  });
}

export function getCurrentUser() {
  return request("/api/auth/me");
}

export function logout() {
  return request("/api/auth/logout", {
    method: "POST",
  });
}
