import { source } from '@/lib/source';
import { createFromSource } from 'fumadocs-core/search/server';

// 정적 export 환경: 빌드 시 검색 인덱스를 정적 파일로 생성.
// Orama 가 한국어(ko) 스테머를 지원하지 않으므로 모든 로케일을 기본 토크나이저(english)로 매핑.
export const revalidate = false;
export const { staticGET: GET } = createFromSource(source, {
  localeMap: {
    ko: 'english',
    en: 'english',
    ja: 'english',
    zh: 'english',
  },
});
