import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { register } from "../api/auth";

export default function RegisterPage() {
    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    async function handleSubmit(e) {
        e.preventDefault();

        try {
            setError("");

            await register(username, email, password);

            navigate("/login");
        } catch (err) {
            setError(err.message);
        }
    }

    return (
        <main>
            <h1>Создать аккаунт</h1>

            <form onSubmit={handleSubmit}>
                <input
                    placeholder="Username"
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                />

                <input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />

                <input
                    type="password"
                    placeholder="Пароль"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                />

                {error && <p>{error}</p>}

                <button type="submit">
                    Зарегистрироваться
                </button>
            </form>
        </main>
    );
}