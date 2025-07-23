document.addEventListener("DOMContentLoaded", function () {
    const departmentSelect = document.getElementById("departmentSelect");
    const postSelect = document.getElementById("postSelect");

    departmentSelect.addEventListener("change", async function () {
        const departmentId = this.value;

        postSelect.innerHTML = '<option value="">-- Выберите должность --</option>';

        if (departmentId) {
            try {
                const response = await fetch(
                    `/Auth/GetPostsByDepartment?departmentId=${departmentId}`
                );
                const posts = await response.json();

                posts.forEach((post) => {
                    const option = document.createElement("option");
                    option.value = post.id;
                    option.textContent = post.title;
                    postSelect.appendChild(option);
                });
            } catch (error) {
                console.error("Ошибка при загрузке должностей:", error);
            }
        }
    });
});
