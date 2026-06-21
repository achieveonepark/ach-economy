import { docs } from '@/.source/server';
import { loader } from 'fumadocs-core/source';
import { i18n } from 'ach-fumadocs-theme';

// baseUrl '/' + i18n → 문서가 로케일 루트에 위치 (/ko, /ko/getting-started ...) — lite-db 와 동일.
export const source = loader({
  baseUrl: '/',
  i18n,
  source: docs.toFumadocsSource(),
});
