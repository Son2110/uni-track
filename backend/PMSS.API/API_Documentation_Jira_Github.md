# API Documentation: Jira & Github Integration

## 1. Jira API

### 1.1. Tạo cấu hình Jira cho Project
- **Endpoint:** `POST /api/jira/config`
- **Request Body:**
```json
{
  "projectId": "<GUID>",
  "jiraUrl": "https://your-domain.atlassian.net",
  "email": "jira-user@example.com",
  "apiToken": "your-jira-api-token",
  "projectKey": "ABC"
}
```
- **Response:**
```json
{
  "jiraConfigId": "<GUID>",
  "projectId": "<GUID>",
  "projectName": "Project Name",
  "jiraUrl": "https://your-domain.atlassian.net",
  "email": "jira-user@example.com",
  "apiTokenMasked": "xxxx****xxxx",
  "projectKey": "ABC",
  "isActive": true,
  "createdAt": "2026-03-25T12:00:00Z",
  "updatedAt": "2026-03-25T12:00:00Z"
}
```

### 1.2. Lấy cấu hình Jira
- **Endpoint:** `GET /api/jira/config/{projectId}`
- **Response:**
```json
{
  "jiraConfigId": "<GUID>",
  "projectId": "<GUID>",
  "projectName": "Project Name",
  "jiraUrl": "https://your-domain.atlassian.net",
  "email": "jira-user@example.com",
  "apiTokenMasked": "xxxx****xxxx",
  "projectKey": "ABC",
  "isActive": true,
  "createdAt": "2026-03-25T12:00:00Z",
  "updatedAt": "2026-03-25T12:00:00Z"
}
```

### 1.3. Cập nhật cấu hình Jira
- **Endpoint:** `PUT /api/jira/config/{projectId}`
- **Request Body:** (chỉ gửi trường cần cập nhật)
```json
{
  "jiraUrl": "https://your-domain.atlassian.net",
  "email": "new-email@example.com",
  "apiToken": "new-token",
  "projectKey": "XYZ",
  "isActive": false
}
```
- **Response:**
```json
{ "message": "Configuration updated successfully" }
```

### 1.4. Xóa cấu hình Jira
- **Endpoint:** `DELETE /api/jira/config/{projectId}`
- **Response:** `204 No Content`

### 1.5. Test kết nối Jira
- **Endpoint:** `POST /api/jira/config/{projectId}/test`
- **Response (thành công):**
```json
{ "message": "Connection successful", "connected": true }
```
- **Response (lỗi):**
```json
{ "error": "Connection failed", "connected": false }
```

### 1.6. Lấy raw Jira issues
- **Endpoint:** `GET /api/jira/fetch/{projectId}`
- **Response:** (raw JSON từ Jira)

### 1.7. Sinh SRS từ Jira (AI, lưu file)
- **Endpoint:** `POST /api/jira/generate-srs/{projectId}`
- **Response:**
```json
{
  "success": true,
  "filePath": "generated-srs/SRS_<projectId>.md"
}
```

---

## 2. SRS API (từ Jira)

### 2.1. Sinh SRS dạng JSON
- **Endpoint:** `GET /api/v1/projects/{projectId}/srs`
- **Response:**
```json
{
  "success": true,
  "data": { /* SRS JSON structure */ }
}
```

### 2.2. Sinh SRS dạng DOCX (AI)
- **Endpoint:** `GET /api/v1/projects/{projectId}/srs/docx?usePaidModel=false&modelOption=`
- **Response:** File DOCX (Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document)

### 2.3. Sinh SRS dạng Markdown (AI)
- **Endpoint:** `GET /api/v1/projects/{projectId}/srs/markdown?usePaidModel=false&modelOption=`
- **Response:** File Markdown (Content-Type: text/markdown)

---

## 3. Github Contribution Report API

### 3.1. Sinh và lưu report
- **Endpoint:** `POST /api/v1/projects/{projectId}/github-reports`
- **Request Body:**
```json
{
  "usePaidModel": false,
  "modelOption": "gpt-4",
  "recentWeeks": 4,
  "includeMermaidDiagrams": true
}
```
- **Response:**
```json
{
  "success": true,
  "data": {
    "reportId": "<GUID>",
    "projectId": "<GUID>",
    "markdownContent": "...",
    "mermaidBlocks": ["..."],
    ...
  }
}
```

### 3.2. Lấy danh sách report
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-reports?take=20`
- **Response:**
```json
{
  "success": true,
  "data": [ { "reportId": "<GUID>", ... }, ... ]
}
```

### 3.3. Lấy report mới nhất
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-reports/latest`
- **Response:**
```json
{
  "success": true,
  "data": { "reportId": "<GUID>", ... }
}
```

### 3.4. Lấy report theo ID
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-reports/{reportId}`
- **Response:**
```json
{
  "success": true,
  "data": { "reportId": "<GUID>", ... }
}
```

### 3.5. Tải Markdown
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-report/markdown?usePaidModel=false&modelOption=&recentWeeks=&includeMermaidDiagrams=`
- **Response:** File Markdown (Content-Type: text/markdown)

### 3.6. Tải Mermaid (.mmd)
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-reports/{reportId}/mermaid`
- **Response:** File Mermaid (Content-Type: text/plain)

### 3.7. Lấy Mermaid JSON blocks
- **Endpoint:** `GET /api/v1/projects/{projectId}/github-reports/{reportId}/mermaid-blocks`
- **Response:**
```json
{
  "success": true,
  "data": [ "mermaid block 1", "mermaid block 2", ... ]
}
```

---

## 4. Github Repo API

### 4.1. CRUD repo
- **GET /api/v1/github-repos?courseId=&userId=**
- **GET /api/v1/github-repos/{id}**
- **POST /api/v1/github-repos**
- **PUT /api/v1/github-repos/{id}**
- **DELETE /api/v1/github-repos/{id}**

### 4.2. Quản lý contributor
- **Thêm contributor:**
  `POST /api/v1/github-repos/{repoId}/contributors/{userId}`
- **Xóa contributor:**
  `DELETE /api/v1/github-repos/{repoId}/contributors/{userId}`

---

## 5. Lưu ý sử dụng
- Tất cả các API đều yêu cầu JWT token (trừ các API public như lấy project).
- Tham khảo thêm mô tả chi tiết và thử trực tiếp tại Swagger UI: `http://localhost:5055/swagger/index.html`
- Response có thể trả về mã lỗi 400/404/502 tuỳ trường hợp, xem chi tiết message trong body.

---

Nếu cần ví dụ cụ thể hơn cho từng API, hãy liên hệ để bổ sung!
