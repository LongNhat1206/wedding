/**
 * ============================================================================
 *  CODE NÀY DÁN THẲNG VÀO GOOGLE SHEET — không cần Cloudflare, không cần
 *  Google Cloud, không cần Service Account/API key gì cả.
 * ============================================================================
 *  Cách cài (5 phút):
 *   1. Mở Google Sheet bạn dùng để lưu lời chúc.
 *   2. Đặt tên tab (dưới cùng) là "LoiChuc". Dòng 1 nhập tiêu đề:
 *      ThoiGian | Ten | LoiChuc
 *   3. Menu Extensions (Tiện ích) → Apps Script.
 *   4. Xoá hết code mẫu, dán toàn bộ nội dung file này vào.
 *   5. Bấm Deploy (Triển khai) → New deployment (Triển khai mới):
 *        - Type: Web app (Ứng dụng web)
 *        - Execute as: Me (Tôi)
 *        - Who has access: Anyone (Bất kỳ ai)
 *      Bấm Deploy. Google sẽ hỏi quyền truy cập Sheet → Cho phép (Authorize).
 *   6. Copy URL dạng: https://script.google.com/macros/s/xxxxx/exec
 *      → đây là URL duy nhất bạn dán vào file HTML thiệp cưới (biến WISH_API_URL).
 *
 *  Vì sao vẫn ẩn được thông tin dù không qua Worker:
 *   - Trình duyệt (F12) chỉ thấy được URL .../exec ở trên — đây là URL bắt
 *     buộc phải công khai để gọi được. Nó KHÔNG hé lộ ID Sheet thật, không có
 *     quyền gì ngoài những gì đoạn code này cho phép.
 *   - Script chỉ có 2 chức năng: đọc toàn bộ danh sách (doGet), và thêm 1
 *     dòng mới hợp lệ (doPost). Không có chức năng sửa/xoá dòng có sẵn →
 *     dù ai gọi thẳng URL bằng tay cũng không chỉnh sửa được lời chúc cũ.
 * ============================================================================
 */

const SHEET_NAME = "LoiChuc";

// ── Danh sách từ khoá cấm — tự thêm/bớt tuỳ ý ──
const BANNED_WORDS = [
  "dm", "vl", "vcl", "clgt", "cc", "djt", "dit me", "dit con me",
  "loz", "lon", "cac", "buoi", "deo", "vailon", "sml", "ngu nhu cho",
  "thang cho", "con cho", "sex", "xxx", "porn",
  // 👉 thêm từ bạn muốn chặn vào đây
];

function normalize(str) {
  return str
    .toString()
    .normalize("NFD").replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/gi, "d")
    .toLowerCase()
    .replace(/[^a-z0-9\s]/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function containsBannedWord(text) {
  const norm = normalize(text);
  return BANNED_WORDS.some(w => norm.includes(normalize(w)));
}

function jsonOut(obj) {
  return ContentService.createTextOutput(JSON.stringify(obj))
    .setMimeType(ContentService.MimeType.JSON);
}

function getSheet() {
  return SpreadsheetApp.getActiveSpreadsheet().getSheetByName(SHEET_NAME);
}

// ── GET: trả về danh sách lời chúc ──
function doGet(e) {
  try {
    const sheet = getSheet();
    const data = sheet.getDataRange().getValues();
    const rows = data.slice(1) // bỏ dòng tiêu đề
      .filter(r => r[1] && r[2])
      .map(r => ({ name: String(r[1]), text: String(r[2]) }))
      .reverse(); // mới nhất lên đầu
    return jsonOut({ ok: true, wishes: rows });
  } catch (err) {
    return jsonOut({ ok: false, error: "Không tải được lời chúc." });
  }
}

// ── POST: nhận lời chúc mới, kiểm tra rồi thêm 1 dòng ──
function doPost(e) {
  const lock = LockService.getScriptLock();
  lock.waitLock(10000);
  try {
    const body = JSON.parse(e.postData.contents);

    // Honeypot: ô ẩn trong form mà người thật không thấy/không điền.
    // Nếu có giá trị -> gần như chắc chắn là bot -> chặn thẳng, không báo lỗi rõ.
    if (body.hp) return jsonOut({ ok: false, error: "Có lỗi xảy ra, vui lòng thử lại." });

    const name = (body.name || "").toString().trim().slice(0, 60);
    const text = (body.text || "").toString().trim().slice(0, 500);

    if (!name || !text) {
      return jsonOut({ ok: false, error: "Vui lòng nhập đầy đủ tên và lời chúc." });
    }
    if (containsBannedWord(name) || containsBannedWord(text)) {
      return jsonOut({ ok: false, error: "Lời chúc chứa từ ngữ không phù hợp, vui lòng chỉnh lại." });
    }

    const props = PropertiesService.getScriptProperties();
    const now = Date.now();

    // Chặn gửi dồn dập: cách nhau tối thiểu 4 giây giữa MỌI lượt gửi (toàn hệ thống).
    // Chặn được bot bắn liên tục hàng loạt request.
    const lastTime = parseInt(props.getProperty("lastSubmitTime") || "0", 10);
    if (now - lastTime < 4000) {
      return jsonOut({ ok: false, error: "Hệ thống đang bận, vui lòng thử lại sau vài giây." });
    }

    // Chặn gửi trùng y hệt nội dung trong 3 phút gần nhất (chặn spam lặp lại 1 câu).
    const lastText = props.getProperty("lastText") || "";
    if (lastText === text && now - lastTime < 3 * 60 * 1000) {
      return jsonOut({ ok: false, error: "Lời chúc này vừa được gửi rồi, cảm ơn bạn!" });
    }

    getSheet().appendRow([new Date(), name, text]);
    props.setProperty("lastSubmitTime", String(now));
    props.setProperty("lastText", text);

    return jsonOut({ ok: true });
  } catch (err) {
    return jsonOut({ ok: false, error: "Có lỗi xảy ra, vui lòng thử lại." });
  } finally {
    lock.releaseLock();
  }
}