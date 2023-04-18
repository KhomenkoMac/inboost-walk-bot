
# Inboost walks bot

This is a test task for Inboost where I create a bot which gives a user stroll-statistics. Using T-SQL I calculate traveled distance using [haversine][haverFormulas] and deal with task-given grouping

## 🛠 Skills
.NET/C#, MS SQL Server/T-SQL, Dapper, SSMS, Viber bots

## Task

На основі даних в таблиці потрібно зробити розрахунки в процедурі, що зберігається (скрипт таблиці MSSQL додається). Опісля створити Viber-бот.




## Roadmap

- [x]  Потрібно розділити данні на окремі прогулянки (прогулянка вважається новою якщо проміжок часу між останнім сигналом від 30 хвилин)
- [x]  Прорахувати відстань пройдену за кожну прогулянку
- [x]  Прорахувати час кожної прогулянки
- [x]  Прорахувати скільки пройшов за день і скільки часу всього гуляв
- [x]  Створити вайбер бота, де по IMEI вивести загальну інформацію про прогулянку ( кількість, кілометраж, тривалість) і ТОП 10 прогулянок по пройденій відстані. Вводимо IMEI, і отримуємо повідомлення, як на фото. Нажимаємо кнопку «Топ 10» і отримуємо інфо

[haverFormulas]: <https://www-marathonus-com.cdn.ampproject.org/v/s/www.marathonus.com/amp/about/blog/using-haversines-with-sql-to-calculate-accurate-distances/?amp_gsa=1&amp_js_v=a9&usqp=mq331AQIUAKwASCAAgM%3D#amp_tf=From%20%251%24s&aoh=16816493406594&referrer=https%3A%2F%2Fwww.google.com&ampshare=https%3A%2F%2Fwww.marathonus.com%2Fabout%2Fblog%2Fusing-haversines-with-sql-to-calculate-accurate-distances%2F>