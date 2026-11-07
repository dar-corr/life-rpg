import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createCharacter } from "../api/characters";

export default function CreateCharacterPage() {
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(e) {
        e.preventDefault();

        const characterName = name.trim();

        if (!characterName || characterName.length > 100) {
            setError("Имя должно содержать от 1 до 100 символов.");
            return;
        }

        setLoading(true);
        setError("");

        try {
            await createCharacter(characterName);
            navigate("/dashboard", { replace: true });
        } catch (err) {
            if (err.status === 401) {
                navigate("/login", { replace: true });
            } else if (err.status === 409) {
                navigate("/dashboard", { replace: true });
            } else {
                setError(err.message);
            }
        } finally {
            setLoading(false);
        }
    }

    return (
        <main>
            <h1>Создание персонажа</h1>

            <p>Начни свою историю в Life RPG.</p>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Имя персонажа"
                    value={name}
                    maxLength={100}
                    onChange={(e) => setName(e.target.value)}
                    required
                />

                {error && <p role="alert">{error}</p>}

                <button type="submit" disabled={loading}>
                    {loading ? "Создание..." : "Создать персонажа"}
                </button>
            </form>
        </main>
    );
}
