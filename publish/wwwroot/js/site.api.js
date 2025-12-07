// simple jQuery AJAX for CRUD
$(function () {
  const base = "/api/products";

  function renderList(items) {
    const $ul = $("#api-products").empty();
    items.forEach(p => {
      const li = $(`<li/>`).text(`${p.id}: ${p.name} - ${p.finalPrice}`);
      const btnDel = $("<button>Delete</button>").on("click", () => deleteProduct(p.id));
      const btnEdit = $("<button>Edit</button>").on("click", () => editPrompt(p));
      li.append(" ", btnEdit, " ", btnDel);
      $ul.append(li);
    });
  }

  function load() {
    $.getJSON(base).done(renderList).fail(err => alert("Load failed: " + err.status));
  }

  function createProduct(name, desc, basePrice, markup, categoryId) {
    $.ajax({
      url: base,
      method: "POST",
      contentType: "application/json",
      data: JSON.stringify({
        Name: name,
        Description: desc,
        BasePrice: parseFloat(basePrice),
        MarkupPercentage: parseInt(markup, 10),
        CategoryId: parseInt(categoryId, 10)
      })
    }).done(() => load()).fail(err => alert("Create failed: " + (err.status || err.statusText)));
  }

  function updateProduct(id, name, desc, basePrice, markup, categoryId) {
    $.ajax({
      url: base + "/" + id,
      method: "PUT",
      contentType: "application/json",
      data: JSON.stringify({
        Id: id,
        Name: name,
        Description: desc,
        BasePrice: parseFloat(basePrice),
        MarkupPercentage: parseInt(markup, 10),
        CategoryId: parseInt(categoryId, 10)
      })
    }).done(() => load()).fail(err => alert("Update failed: " + (err.status || err.statusText)));
  }

  function deleteProduct(id) {
    $.ajax({ url: base + "/" + id, method: "DELETE" })
      .done(() => load()).fail(err => alert("Delete failed: " + (err.status || err.statusText)));
  }

  function editPrompt(p) {
    const name = prompt("Name:", p.name);
    const desc = prompt("Description:", p.description || "");
    const basePrice = prompt("BasePrice:", p.basePrice || 0);
    const markup = prompt("MarkupPercentage:", p.markupPercentage || 0);
    const categoryId = prompt("CategoryId:", p.categoryId || 1);
    if (name != null) updateProduct(p.id, name, desc, basePrice, markup, categoryId);
  }

  // wire UI
  $("#btn-create").on("click", function () {
    const name = $("#p-name").val();
    const desc = $("#p-desc").val();
    const basePrice = $("#p-price").val();
    const markup = $("#p-markup").val();
    const categoryId = $("#p-category").val() || "1";
    createProduct(name, desc, basePrice, markup, categoryId);
  });

  // initial load
  load();
});