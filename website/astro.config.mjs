// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightLinksValidator from 'starlight-links-validator';
import { visit } from 'unist-util-visit';

const repository = 'https://github.com/QuinntyneBrown/CodeGenerator';
const base = '/CodeGenerator';

/**
 * Prefixes site-absolute links in markdown with the configured base.
 *
 * Astro does not rewrite links written inside markdown, so `/cli/scaffold/` would
 * resolve above the project site's root and 404. Handling it here keeps both the
 * authored pages and the generated reference free of the repository name, so the
 * base can change in one place.
 */
function rehypeBaseUrls() {
  return (tree) => {
    visit(tree, 'element', (node) => {
      const attribute = node.tagName === 'a' ? 'href' : node.tagName === 'img' ? 'src' : null;
      if (!attribute) return;

      const value = node.properties?.[attribute];
      if (typeof value !== 'string') return;

      // Site-absolute only: leave protocol-relative, external, anchor, and
      // already-prefixed links alone so the transform is idempotent.
      if (!value.startsWith('/') || value.startsWith('//')) return;
      if (value === base || value.startsWith(`${base}/`)) return;

      node.properties[attribute] = `${base}${value}`;
    });
  };
}

export default defineConfig({
  // Published as a GitHub Pages project site, so every route is served under /CodeGenerator.
  site: 'https://quinntynebrown.github.io',
  base,
  trailingSlash: 'always',

  markdown: {
    rehypePlugins: [rehypeBaseUrls],
  },

  integrations: [
    starlight({
      title: 'create-code-cli',
      description:
        'Documentation for create-code-cli, the CodeGenerator command line tool for scaffolding solutions, projects, and full-stack applications.',
      social: [
        { icon: 'github', label: 'GitHub', href: repository },
      ],
      editLink: {
        baseUrl: `${repository}/edit/main/website/`,
      },
      lastUpdated: true,
      customCss: ['./src/styles/custom.css'],

      // Fails the build on a broken internal link or anchor. Required rather than
      // optional: `base` breaks any hand-written absolute link, and a generated
      // reference of this size cannot be link-checked by eye.
      plugins: [starlightLinksValidator({ errorOnRelativeLinks: false })],

      sidebar: [
        {
          label: 'Start here',
          items: [
            { label: 'What is create-code-cli?', slug: 'overview' },
            { label: 'Installation', slug: 'install' },
            { label: 'Your first generator project', slug: 'start/first-project' },
            { label: 'Scaffold a workspace from YAML', slug: 'start/first-scaffold' },
            { label: 'Install the agent skill', slug: 'start/agent-skill' },
          ],
        },
        // Starlight 0.39 removed `label` alongside `autogenerate`; a labelled group
        // now wraps the autogenerate entry in its `items` array.
        {
          label: 'Guides',
          items: [{ autogenerate: { directory: 'guides' } }],
        },
        {
          label: 'CLI reference',
          items: [{ autogenerate: { directory: 'cli' } }],
        },
        {
          label: 'Configuration',
          items: [{ autogenerate: { directory: 'config' } }],
        },
        {
          label: 'scaffold.yaml reference',
          items: [{ autogenerate: { directory: 'scaffold' } }],
        },
        {
          label: 'Reference',
          items: [{ autogenerate: { directory: 'reference' } }],
        },
        {
          label: 'Troubleshooting',
          items: [{ autogenerate: { directory: 'troubleshooting' } }],
        },
        {
          label: 'Project docs',
          collapsed: true,
          items: [{ autogenerate: { directory: 'project' } }],
        },
      ],
    }),
  ],
});
