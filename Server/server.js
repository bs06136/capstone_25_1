const express = require('express');
const path    = require('path');

const app  = express();
const PORT = 80;

// 정적 파일 제공
app.use(express.static(path.join(__dirname, 'public')));

// “/” 요청 → 메인 페이지
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// “/download” 요청 → 다운로드 전용 페이지
app.get('/download', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'download.html'));
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`OverCloud Link Server 실행중: http://localhost:${PORT}`);
});
