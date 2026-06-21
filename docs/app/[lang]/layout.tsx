import { DocsLayout } from 'fumadocs-ui/layouts/docs';
import { RootProvider } from 'fumadocs-ui/provider/next';
import { i18nProvider, staticSearchOptions } from 'ach-fumadocs-theme';
import type { ReactNode } from 'react';
import { source } from '@/lib/source';
import { baseOptions } from '../layout.config';

export default async function Layout({
  params,
  children,
}: {
  params: Promise<{ lang: string }>;
  children: ReactNode;
}) {
  const { lang } = await params;
  return (
    <RootProvider i18n={i18nProvider(lang)} search={{ options: staticSearchOptions }}>
      <DocsLayout tree={source.pageTree[lang]} {...baseOptions}>
        {children}
      </DocsLayout>
    </RootProvider>
  );
}
