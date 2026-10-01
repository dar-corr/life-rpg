import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    getCurrentUser,
    logout
} from "../api/auth";

export default function DashboardPage() {
    const navigate = useNavigate();

    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function loadUser() {
            try {
                const currentUser =
                    await getCurrentUser();

                setUser(currentUser);
            } catch {
                navigate("/login");
            } finally {
                setLoading(false);
            }
        }

        loadUser();
    }, [navigate]);

    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    if (loading) {
        return <p>Загрузка...</p>;
    }

    return (
        <main>
            <h1>Life RPG</h1>

            <h2>
                Добро пожаловать, {user.userName}
            </h2>

            <p>{user.email}</p>

            <button onClick={handleLogout}>
                Выйти
            </button>
        </main>
    );
}