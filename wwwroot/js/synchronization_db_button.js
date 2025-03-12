let temp_data = document.getElementById("sync_bt");

async function sync_db() {
    temp_data.addEventListener('click', async function (a) {
        console.log("BT_TEMP_DATA: " + temp_data);
        try {
            var response = await fetch("https://localhost:7205/Home/DBSync", { method: "POST" });
            if (response.ok) {
                alert("������������� ������ �������.");
            } else {
                alert("������������� �� �� �������.");
            }
        } catch (error) {
            console.error("������ ��� �������������: ", error);
            alert("��������� ������ ��� �������������.");
        }
    });
}
sync_db();