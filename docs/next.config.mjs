import { createMDX } from 'fumadocs-mdx/next';
import { achNextConfig } from 'ach-fumadocs-theme/next';

const withMDX = createMDX();

// GitHub Pages 정적 export. basePath = /ach-economy (repo 이름 기준).
export default withMDX(achNextConfig({ repo: 'ach-economy' }));
