(() => {
  function escapeHtml(str) {
    return (str || "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function closePanel(panel) {
    panel.hidden = true;
    panel.innerHTML = "";
    panel.dataset.open = "0";
  }

  async function runAiSearch(input, panel) {
    const q = (input.value || "").trim();
    if (!q) return;

    const url = input.dataset.aiSearchUrl || "/api/ai-search";

    panel.hidden = false;
    panel.dataset.open = "1";
    panel.innerHTML = `
      <div class="cd-aiSearchPanel__hdr">
        <div class="cd-aiSearchPanel__title"><span class="cd-aiSearchPanel__spinner"></span>Gemini AI</div>
        <div class="cd-aiSearchPanel__actions">
          <button type="button" class="cd-aiSearchPanel__btn" data-cd-ai-close>Close</button>
        </div>
      </div>
      <div class="cd-aiSearchPanel__body cd-aiSearchPanel__muted">Thinking…</div>
    `;

    input.setAttribute("aria-busy", "true");
    input.disabled = true;

    try {
      const res = await fetch(`${url}?q=${encodeURIComponent(q)}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
      });

      const text = await res.text();
      if (!res.ok) {
        panel.querySelector(".cd-aiSearchPanel__body").innerHTML =
          `<div class="cd-aiSearchPanel__muted">Request failed (${res.status}).</div><div class="cd-aiSearchPanel__muted">${escapeHtml(text)}</div>`;
        return;
      }

      const data = JSON.parse(text);
      const answer = (data && data.answer) ? String(data.answer) : "";
      const meta = [];
      if (data && typeof data.elapsedMs === "number") meta.push(`${data.elapsedMs}ms`);

      panel.innerHTML = `
        <div class="cd-aiSearchPanel__hdr">
          <div class="cd-aiSearchPanel__title">Gemini AI ${meta.length ? `• ${escapeHtml(meta.join(" • "))}` : ""}</div>
          <div class="cd-aiSearchPanel__actions">
            <button type="button" class="cd-aiSearchPanel__btn" data-cd-ai-copy>Copy</button>
            <button type="button" class="cd-aiSearchPanel__btn" data-cd-ai-close>Close</button>
          </div>
        </div>
        <div class="cd-aiSearchPanel__body">${escapeHtml(answer) || '<span class="cd-aiSearchPanel__muted">No answer returned.</span>'}</div>
      `;

      const copyBtn = panel.querySelector("[data-cd-ai-copy]");
      if (copyBtn) {
        copyBtn.addEventListener("click", async () => {
          try {
            await navigator.clipboard.writeText(answer);
            copyBtn.textContent = "Copied";
            setTimeout(() => (copyBtn.textContent = "Copy"), 900);
          } catch {
            copyBtn.textContent = "Copy failed";
            setTimeout(() => (copyBtn.textContent = "Copy"), 900);
          }
        });
      }
    } catch (err) {
      panel.querySelector(".cd-aiSearchPanel__body").innerHTML =
        `<div class="cd-aiSearchPanel__muted">Error: ${escapeHtml(err && err.message ? err.message : String(err))}</div>`;
    } finally {
      input.disabled = false;
      input.removeAttribute("aria-busy");
      input.focus();
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("cdTopbarAiSearch");
    const panel = document.getElementById("cdTopbarAiSearchPanel");
    if (!input || !panel) return;

    // Enter triggers Gemini AI search
    input.addEventListener("keydown", (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        runAiSearch(input, panel);
      } else if (e.key === "Escape") {
        closePanel(panel);
      }
    });

    // Close buttons inside panel
    panel.addEventListener("click", (e) => {
      const btn = e.target && e.target.closest ? e.target.closest("[data-cd-ai-close]") : null;
      if (btn) closePanel(panel);
    });

    // Click outside closes panel
    document.addEventListener("click", (e) => {
      if (panel.hidden) return;
      const withinSearch = input.closest(".cd-topbar__search");
      if (!withinSearch) return;
      if (withinSearch.contains(e.target)) return;
      closePanel(panel);
    });
  });
})();
