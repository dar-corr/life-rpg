import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getCurrentUser, logout } from "../api/auth";
import { getCharacter } from "../api/characters";

export default function DashboardPage() {
    const navigate = useNavigate();

    const [user, setUser] = useState(null);
    const [character, setCharacter] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let cancelled = false;

        async function loadData() {
            try {
                const currentUser = await getCurrentUser();
                const currentCharacter = await getCharacter();

                if (!cancelled) {
                    setUser(currentUser);
                    setCharacter(currentCharacter);
                }
            } catch (err) {
                if (cancelled) return;

                if (err.status === 401) {
                    navigate("/login", { replace: true });
                } else if (err.status === 404) {
                    navigate("/character/create", { replace: true });
                } else {
                    setError(err.message);
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        loadData();

        return () => {
            cancelled = true;
        };
    }, [navigate]);

    async function handleLogout() {
        try {
            await logout();
            navigate("/login", { replace: true });
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) {
        return <p>Загрузка...</p>;
    }

    if (error) {
        return <p role="alert">Ошибка: {error}</p>;
    }

    if (!user || !character) {
        return null;
    }

    return (
        <main>
            <h1>Life RPG</h1>

            <p>Аккаунт: {user.userName}</p>

            <section>
                <h2>{character.name}</h2>

                <p>Уровень: {character.level}</p>
                <p>Опыт: {character.totalXp} XP</p>
            </section>

            <button onClick={handleLogout}>
                Выйти
            </button>
        </main>
    );
}
