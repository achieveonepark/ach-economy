'use client';

import { useEffect } from 'react';

// 정적 호스팅(GitHub Pages)에는 서버 리다이렉트가 없으므로 클라이언트에서 기본 로케일로 보냅니다.
const target = `${process.env.NEXT_PUBLIC_BASE_PATH ?? ''}/ko`;

export default function RootRedirect() {
  useEffect(() => {
    window.location.replace(target);
  }, []);

  return (
    <noscript>
      <meta httpEquiv="refresh" content={`0; url=${target}`} />
    </noscript>
  );
}
