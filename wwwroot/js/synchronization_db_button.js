let temp_data = document.getElementById("sync_bt");

async function sync_db() {
    temp_data.addEventListener('click', async function (a) {
        console.log("BT_TEMP_DATA: " + temp_data);
        try {
            var response = await fetch("https://localhost:7205/Home/DBSync", { method: "POST" });
            if (response.ok) {
                alert("Синхронизация прошла успешно.");
            } else {
                alert("Синхронизация БД не удалась.");
            }
        } catch (error) {
            console.error("Ошибка при синхронизации: ", error);
            alert("Произошла ошибка при синхронизации.");
        }
    });
}
sync_db();