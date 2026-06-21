import './global.css';
import type { ReactNode } from 'react';

// 루트는 html/body 만 담당. 로케일별 RootProvider 는 app/[lang]/layout.tsx 에 있음.
export default function Layout({ children }: { children: ReactNode }) {
  return (
    <html lang="ko" suppressHydrationWarning>
      <body className="flex flex-col min-h-screen">{children}</body>
    </html>
  );
}
