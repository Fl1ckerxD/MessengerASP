// Здесь будет логика для фильтрации, пагинации и обработки кнопок
document.addEventListener("DOMContentLoaded", function () {
  const acceptButtons = document.querySelectorAll(".btn-accept");
  const rejectButtons = document.querySelectorAll(".btn-reject");

  acceptButtons.forEach((button) => {
    button.addEventListener("click", async function () {
      const row = this.closest("tr");
      // Логика принятия пользователя
      const userId = row.dataset.id;
      const response = await fetch("/Admin/Request/Accept", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(userId),
      });
      if (!response.ok) console.log(response.statusText);

      alert("Пользователь принят: " + row.cells[0].textContent);
      row.remove();
    });
  });

  rejectButtons.forEach((button) => {
    button.addEventListener("click", async function () {
      const row = this.closest("tr");
      // Логика отклонения пользователя
      if (confirm("Отклонить запрос от " + row.cells[0].textContent + "?")) {
        const userId = row.dataset.id;
        const response = await fetch(`/Admin/Request/Reject`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
          },
          body: JSON.stringify(userId),
        });
        if (!response.ok) console.log(response.statusText);
        row.remove();
      }
    });
  });

  // Простая фильтрация по поиску
  document.getElementById("search").addEventListener("input", function () {
    const searchTerm = this.value.toLowerCase();
    const rows = document.querySelectorAll("#requests-table tbody tr");

    rows.forEach((row) => {
      const name = row.cells[0].textContent.toLowerCase();
      const position = row.cells[2].textContent.toLowerCase();

      if (name.includes(searchTerm) || position.includes(searchTerm)) {
        row.style.display = "";
      } else {
        row.style.display = "none";
      }
    });
  });
});
