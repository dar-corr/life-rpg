import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../api/auth";

export default function LoginPage() {
    const navigate = useNavigate();

    const [loginValue, setLoginValue] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    async function handleSubmit(e) {
        e.preventDefault();

        try {
            setError("");

            await login(loginValue, password);

            navigate("/dashboard");
        } catch {
            setError("Неверный логин или пароль.");
        }
    }

    return (
        <main>
            <h1>Вход</h1>

            <form onSubmit={handleSubmit}>
                <input
                    placeholder="Email или Username"
                    value={loginValue}
                    onChange={e =>
                        setLoginValue(e.target.value)
                    }
                />

                <input
                    type="password"
                    placeholder="Пароль"
                    value={password}
                    onChange={e =>
                        setPassword(e.target.value)
                    }
                />

                {error && <p>{error}</p>}

                <button type="submit">
                    Войти
                </button>
            </form>
        </main>
    );
}