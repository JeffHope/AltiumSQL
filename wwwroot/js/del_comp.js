let btn_delete_comp = document.getElementById("del_comp_bt");
let modal_del_comp = document.getElementsByClassName("modal_del_comp")[0];
let modal_del_comp_content = document.getElementsByClassName("modal_del_comp_content")[0];
let close_del_comp = document.getElementById("close_id_del_comp");
let btn_send_delete = document.getElementById("send_data_del");

let open_modal = () =>
{
    modal_del_comp.style.display = "block";
}
let close_modal = () =>
{
    modal_del_comp_content.innerHTML = "";
    modal_del_comp.style.display = "none";
}
close_del_comp.onclick = function ()
{
    close_modal();
    
}    

export function del_comp(table_name, data_json) {
    btn_delete_comp.onclick = function ()
    {
        open_modal();
        let json =
        {
            "table": table_name,
            "comp": {}
        }
        modal_del_comp_content.appendChild(close_del_comp);
        let table_label = document.createElement('label');
        table_label.textContent = `Таблица \"${table_name}\"`;
        table_label.style.fontWeight = 'bold';
        table_label.style.fontSize = '22px';
        
        modal_del_comp_content.appendChild(table_label);
        let main_label = document.createElement('label');
        main_label.textContent = `Выберите компонент по значению PartNumber, который хотите удалить: \n`;
        modal_del_comp_content.appendChild(main_label);
        let cnt_btn_id = null;
        data_json.forEach((array) => {
            let btn_partnumber = document.createElement('button');
            cnt_btn_id++;
            btn_partnumber.id = `btn_delete${cnt_btn_id}`;
            btn_partnumber.classList.add('.btn_delete_class');
            btn_partnumber.textContent = array["PartNumber"];
            btn_partnumber.style.display = 'block'; 
            btn_partnumber.style.marginBottom = '20px'; 
            btn_partnumber.style.width = '100%';
            btn_partnumber.style.padding = '10px'; 
            btn_partnumber.style.textAlign = 'left'
            modal_del_comp_content.appendChild(btn_partnumber);
            btn_partnumber.addEventListener('click', () =>
            {
                console.log(`Вы выбрали компонент ${btn_partnumber.textContent}`);
                btn_send_delete.onclick = async function ()
                {
                    json.comp = array;
                    let promise = await fetch("https://localhost:7205/Home/RemoveComponent",
                        {
                            method: "POST",
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(json)
                        });
                    if (promise.ok)
                    {
                        close_modal();
                        await loadTableData(json.table);
                    }
                }
                
            });
        });
        
        modal_del_comp_content.appendChild(btn_send_delete);
        
    }
    /*ПРОВЕРИТЬ ОБНОВЛЕНИЕ ТАБЛИЦЫ НА ВЕБ!!!*/
    //loadTableData(table_name);
}
