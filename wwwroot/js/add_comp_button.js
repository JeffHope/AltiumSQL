let modal_new_comp = document.getElementsByClassName("modal_new_comp")[0];
let span_close_new_comp = document.getElementById("close_first_new_comp");
let btn_send_new_comp = document.getElementById("send_new_comp_data");
let addBt = document.getElementById("add_comp_bt");
let formContainer = document.getElementsByClassName("modal_new_comp_content")[0];

let open_modal = () => {
    modal_new_comp.style.display = "block";
}

let close_modal = () => {
    formContainer.innerHTML = ""; // Очищаем содержимое модального окна
    modal_new_comp.style.display = "none";
}

span_close_new_comp.onclick = function () {
    close_modal();
}

export function createAddComponentForm(headers, table_name) {
    if (headers.length != 0) {
        addBt.onclick = async function () {
            open_modal();
            formContainer.innerHTML = ""; 
            let table_name_label = document.createElement('label');
            table_name_label.textContent = `Добавление компонента в таблицу:\n ${table_name}`;
            table_name_label.style.fontWeight = 'bold';
            table_name_label.style.marginBottom = '20px';
            table_name_label.style.display = 'block';
            formContainer.appendChild(table_name_label);
            let json = {
                table: table_name,
                comp: {} 
            };
            headers.forEach(header => {
                let new_label = document.createElement('label');
                new_label.textContent = header;
                new_label.style.display = 'block';
                new_label.style.marginTop = '10px';
                formContainer.appendChild(new_label);

                let new_input = document.createElement('input');
                new_input.name = header;
                new_input.type = "text";
                new_input.placeholder = `Введите ${header}`;
                new_input.classList.add("new-inputs");
                new_input.style.width = '100%';
                new_input.style.marginBottom = '10px';
                formContainer.appendChild(new_input);
            });

            formContainer.appendChild(span_close_new_comp);
            formContainer.appendChild(btn_send_new_comp);
            modal_new_comp.appendChild(formContainer);

            // Обработчик для кнопки отправки
            btn_send_new_comp.onclick = async () => {
                let inputs = document.querySelectorAll(".new-inputs");
                let allFilled = true;

                inputs.forEach((item) => {
                    if (item.value.trim() === "") {
                        allFilled = false;
                    }
                });

                if (!allFilled) {
                    alert("Пожалуйста, заполните все поля.");
                    return;
                }

                // Заполняем JSON данными из полей ввода
                inputs.forEach((item) => {
                    let field_name = item.name;
                    let field_value = item.value.trim();
                    json.comp[field_name] = field_value; // Теперь json.comp гарантированно существует
                });

                console.log("JSON NEW COMPONENT: " + JSON.stringify(json));

                try {
                    let response = await fetch("https://localhost:7205/Home/AddNewComponent", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(json)
                    });

                    if (response.ok) {
                        close_modal();
                        loadTableData(json.table); // Предполагается, что функция loadTableData существует
                    } else {
                        console.error("Ошибка при отправке данных");
                    }
                } catch (error) {
                    console.error("Ошибка при отправке запроса:", error);
                }
            };
        }
    } else {
        import("./get_tables.js").then((module) => {
            module.getJson();
        });
    }
}