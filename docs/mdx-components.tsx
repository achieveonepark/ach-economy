import defaultMdxComponents from 'fumadocs-ui/mdx';
import { mdxComponents } from 'ach-fumadocs-theme';
import type { MDXComponents } from 'mdx/types';

export function getMDXComponents(components?: MDXComponents): MDXComponents {
  return {
    ...defaultMdxComponents,
    ...mdxComponents,
    ...components,
  };
}
