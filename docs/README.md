# Kobold Documentation

This directory contains the Kobold framework documentation, built with Jekyll and the Just the Docs theme.

## Viewing Locally

To preview the documentation locally:

1. **Install Ruby** (if not already installed)
   - Windows: [RubyInstaller](https://rubyinstaller.org/)
   - macOS: `brew install ruby`
   - Linux: Use your package manager

2. **Install dependencies:**
   ```bash
   cd docs
   bundle install
   ```

3. **Run the development server:**
   ```bash
   bundle exec jekyll serve
   ```

4. **View the site:**
   Open your browser to `http://localhost:4000/Kobold/`

## Building the Site

To build the static site:

```bash
bundle exec jekyll build
```

The generated site will be in `docs/_site/`.

## GitHub Pages

The documentation is automatically deployed to GitHub Pages when changes are pushed to the main branch.

**Live site:** https://toricook.github.io/Kobold/

## Theme

This site uses the [Just the Docs](https://just-the-docs.github.io/just-the-docs/) theme with a custom Kobold color scheme.

## Adding New Pages

To add a new documentation page:

1. Create a `.md` file in the appropriate directory
2. Add YAML front matter at the top:
   ```yaml
   ---
   layout: default
   title: Page Title
   parent: Parent Section Name  # Optional
   nav_order: 1  # Optional
   ---
   ```
3. Write your content in Markdown

See existing pages for examples.
