# Ach Economy 문서

[fumadocs](https://fumadocs.dev) + `ach-fumadocs-theme` 기반 문서 사이트입니다.

## 개발

```bash
npm install   # postinstall에서 fumadocs-mdx가 .source를 생성
npm run dev    # http://localhost:3000
npm run build
```

## 구조

- `content/docs/` — 문서 본문(MDX)과 사이드바 정의(`meta.json`)
- `app/` — Next.js App Router + fumadocs 레이아웃
- 사이드바 카테고리: **소개**(소개·시작하기) / **사용법**(API·변경 내역)

## 테마 연결

`ach-fumadocs-theme`는 `app/global.css`에서 CSS import로 연결돼 있습니다.
테마의 실제 진입점이 다르면 그 한 줄만 맞추면 됩니다. JS 옵션 프리셋을 제공한다면
`app/layout.config.tsx`의 `baseOptions`에 spread 하세요.
