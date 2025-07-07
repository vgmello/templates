# UI Design Best Practices Guide for LLMs

## Table of Contents

1. [Starting from Scratch](#starting-from-scratch)
2. [Hierarchy is Everything](#hierarchy-is-everything)
3. [Layout and Spacing](#layout-and-spacing)
4. [Designing Text](#designing-text)
5. [Working with Color](#working-with-color)
6. [Creating Depth](#creating-depth)
7. [Working with Images](#working-with-images)
8. [Finishing Touches](#finishing-touches)
9. [Leveling Up](#leveling-up)

---

## Starting from Scratch

### 1. Start with a feature, not a layout

**PRINCIPLE**: Always begin by designing individual features rather than trying to design the entire application shell first.

**THE PROBLEM**: When starting a new design project, most people immediately think about the overall layout structure (navigation bars, sidebars, page containers). This leads to getting stuck because you're trying to make layout decisions without knowing what content will fill those areas.

**QUESTIONS TO AVOID**:
- Should it have a top nav or sidebar?
- Should navigation items be on the left or right?
- Should the page content be in a container or full-width?
- Where should the logo go?

**THE SOLUTION**: Start with actual functionality first. Design a specific feature that users will interact with.

**IMPLEMENTATION**:
```
Instead of designing the shell, pick one feature to design first.

For a flight booking service, start with "searching for a flight":
- Field for departure city
- Field for destination city  
- Field for departure date
- Field for return date
- Button to perform the search

Build this feature completely before worrying about navigation or page layout.
```

**VISUAL GUIDELINES**:
- Create a focused, single-purpose interface element
- Design it as if it's the only thing on the page
- Make it fully functional and polished
- Only then consider how it fits into the larger application

**REAL EXAMPLE**: Google's homepage focuses primarily on search functionality - the core feature - rather than complex navigation or layout elements.

**LLM INSTRUCTION**: When asked to design a new application or website, always ask "What is the primary action users need to take?" and design that feature first. Ignore layout concerns until the core functionality is designed.

### 2. Detail comes later

**PRINCIPLE**: Focus on core functionality and layout before getting caught up in visual details like fonts, colors, shadows, and icons.

**THE PROBLEM**: Designers often get distracted by low-level visual details (typefaces, shadows, icons) in the early stages, which prevents them from solving the bigger structural and functional problems.

**WHAT TO AVOID EARLY**:
- Choosing specific fonts
- Adding drop shadows
- Designing custom icons
- Selecting perfect colors
- Fine-tuning spacing

**THE SOLUTION**: Work in low-fidelity first, then progressively add detail.

**IMPLEMENTATION**:
```
Stage 1: Paper sketches or thick Sharpie wireframes
- Focus on layout and functionality
- Use boxes and lines to represent content
- Impossible to get caught up in details

Stage 2: Grayscale digital mockups  
- Force yourself to solve problems with spacing, contrast, and size
- Create clear hierarchy without relying on color
- Build strong foundation before adding visual flourishes

Stage 3: Add color and polish
- Only after layout and hierarchy are solid
- Colors should enhance, not create the hierarchy
```

**VISUAL GUIDELINES**:
- Hold the color: Design in grayscale first
- Use spacing, contrast, and size for hierarchy
- Sketches and wireframes are disposable - use them to explore quickly
- Don't over-invest in early mockups

**REAL EXAMPLE**: The comparison shown between grayscale and colored signup forms demonstrates how grayscale forces better use of spacing and contrast to create hierarchy.

**LLM INSTRUCTION**: When creating UI designs, always start with grayscale wireframes. Use layout, typography scale, and spacing to create visual hierarchy before introducing any colors. Only add color as a final enhancement step.

### 3. Don't design too much

**PRINCIPLE**: Don't design every single feature and edge case before starting implementation. Work in short cycles.

**THE PROBLEM**: Trying to figure out every feature interaction and edge case in the abstract leads to analysis paralysis and frustration. Complex questions like "How should this screen look if the user has 2000 contacts?" are nearly impossible to answer without real implementation.

**EDGE CASE QUESTIONS TO AVOID EARLY**:
- How should this screen look if the user has 2000 contacts?
- Where should the error message go in this form?
- How should this calendar look when there are two events scheduled at the same time?

**THE SOLUTION**: Work in cycles - design simple versions first, then iterate.

**IMPLEMENTATION**:
```
Cycle 1: Design simple version
- Focus on the happy path
- Design for normal use cases
- Keep features minimal

Cycle 2: Build and test
- Implement the simple version
- Discover real complexity through use
- Identify actual problems, not theoretical ones

Cycle 3: Iterate and improve
- Fix real issues that emerged
- Add complexity only when needed
- Continue cycling until no problems remain
```

**VISUAL GUIDELINES**:
- Expect features to be hard to build
- Design the smallest useful version first  
- If a feature is "nice-to-have", design it later
- Always have something you can ship

**REAL EXAMPLE**: A commenting system without attachments is better than no commenting system at all. Build the simple version first, then add complexity.

**LLM INSTRUCTION**: When designing features, always start with the minimal viable interface that solves the core use case. Avoid designing for edge cases until they become real implementation problems. Focus on what can be shipped first.

### 4. Choose a personality

**PRINCIPLE**: Every design has a personality. Deliberately choose the personality that fits your brand and audience.

**THE PROBLEM**: Many designers ignore the personality aspect of design, but every design choice communicates something about the brand's character.

**PERSONALITY FACTORS**:
- **Font choice**: Serif fonts feel elegant/classic, rounded sans-serif feels playful, neutral sans-serif feels professional
- **Color choice**: Blue feels safe and familiar, gold feels expensive and sophisticated, pink feels fun and casual
- **Border radius**: Small radius feels neutral, large radius feels playful, no radius feels serious/formal
- **Language tone**: Formal language feels professional, casual language feels friendly

**IMPLEMENTATION**:
```
Step 1: Define your target personality
- Professional and trustworthy (banking, finance)
- Fun and playful (startups, social apps)
- Elegant and sophisticated (luxury brands)

Step 2: Choose consistent elements
- Typography that matches the personality
- Color palette that evokes the right emotions
- Consistent border radius throughout
- Appropriate language tone

Step 3: Stay consistent
- Don't mix square corners with rounded corners
- Don't mix formal language with casual language
- Ensure all elements support the same personality
```

**VISUAL GUIDELINES**:
- Serif fonts = elegant, classic
- Rounded sans-serif = playful, friendly
- Neutral sans-serif = professional, clean
- Blue = safe, trustworthy
- Gold = expensive, sophisticated
- Pink = fun, casual
- Large border radius = playful
- No border radius = serious, formal

**REAL EXAMPLE**: A banking site uses formal language ("To confirm your identity, please provide...") while a casual site uses friendly language ("Sweet, thanks Steve! Just to make sure this is really you...")

**LLM INSTRUCTION**: Before designing any interface, first define the personality you want to convey. Then ensure all design choices (fonts, colors, border radius, language) consistently support that personality. Don't mix elements that send conflicting personality signals.

### 5. Limit your choices

**PRINCIPLE**: Create design systems in advance to avoid decision paralysis and maintain consistency.

**THE PROBLEM**: Having unlimited choices leads to decision paralysis and inconsistent designs. Questions like "Should this text be 12px or 13px?" waste time and create inconsistencies.

**DECISION PARALYSIS EXAMPLES**:
- Should this text be 12px or 13px?
- Should this box shadow have 10% or 15% opacity?
- Should this avatar be 24px or 25px tall?
- Should I use medium or semibold font weight?
- Should this headline have 18px or 20px bottom margin?

**THE SOLUTION**: Define systems in advance with limited, predetermined options.

**IMPLEMENTATION**:
```
Step 1: Define your systems upfront
- Color palette: 8-10 shades of each color
- Type scale: 12px, 14px, 16px, 18px, 20px, 24px, 32px, 48px
- Spacing scale: 4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px
- Border radius: 0px, 2px, 4px, 8px

Step 2: Use process of elimination
- Pick your best guess from your system
- Compare with adjacent options
- Choose the one that looks best
- If an outside option looks better, reconsider your system

Step 3: Systematize everything
- Font sizes and weights
- Line heights
- Colors
- Margins and padding
- Widths and heights
- Box shadows
- Border radius and width
- Opacity values
```

**VISUAL GUIDELINES**:
- Create predefined color palettes instead of using color pickers
- Use consistent spacing scale (multiples of 4px or 8px)
- Define typography scale in advance
- Limit choices to 3-5 options for any given decision

**REAL EXAMPLE**: Instead of having infinite blue options, create a palette of 10 blue shades and choose from those. This makes decisions faster and designs more consistent.

**LLM INSTRUCTION**: Before starting any design project, define your design systems (colors, typography, spacing, etc.) with limited options. When making design decisions, only choose from your predefined system. This prevents decision paralysis and ensures consistency.

---

## Hierarchy is Everything

### 6. Not all elements are equal

**PRINCIPLE**: Visual hierarchy is the most effective tool for making something feel "designed". Deliberately de-emphasize secondary information and highlight what's most important.

**THE PROBLEM**: When everything in an interface competes for attention equally, it feels noisy and chaotic. Users can't tell what actually matters.

**THE SOLUTION**: Create clear hierarchy by making conscious decisions about what should stand out and what should fade into the background.

**VISUAL GUIDELINES**:
- Make important elements more prominent
- De-emphasize secondary and tertiary information
- Use contrast, size, and positioning strategically
- Ensure the most critical information draws the eye first

**REAL EXAMPLE**: In a dashboard interface, total balance figures should be more prominent than timestamps or minor account details.

**LLM INSTRUCTION**: When designing any interface, first identify the hierarchy of importance for all elements. Then use visual techniques (size, color, weight, spacing) to make this hierarchy obvious to users. Primary content should dominate, secondary content should be visible but not competing, and tertiary content should be present but unobtrusive.

### 7. Size isn't everything

**PRINCIPLE**: Don't rely solely on font size to control hierarchy. Use font weight and color to create better hierarchy with more reasonable sizing.

**THE PROBLEM**: Using only font size for hierarchy often leads to primary content that's too large and secondary content that's too small to read comfortably.

**THE SOLUTION**: Use multiple techniques together for better hierarchy control.

**IMPLEMENTATION**:
```
Instead of just varying size (16px, 14px, 12px), use:

Primary content: 
- Normal size (16px)
- Heavier font weight (600-700)
- Darker color

Secondary content:
- Same or slightly smaller size (16px or 14px)  
- Normal font weight (400-500)
- Lighter color (grey)

Tertiary content:
- Smaller size (14px)
- Normal font weight (400)
- Lightest color (light grey)
```

**COLOR HIERARCHY GUIDELINES**:
- **Dark color** for primary content (headlines, important text)
- **Grey color** for secondary content (dates, metadata)
- **Light grey** for tertiary content (captions, footnotes)

**FONT WEIGHT GUIDELINES**:
- **Normal weight (400-500)** for most text
- **Heavier weight (600-700)** for text you want to emphasize
- **Avoid weights under 400** for UI text (too hard to read at small sizes)

**REAL EXAMPLE**: A product card with title in bold dark text, description in normal grey text, and price in normal dark text creates clear hierarchy without extreme size differences.

**LLM INSTRUCTION**: When creating text hierarchy, start with reasonable font sizes for readability, then use weight (bold vs normal) and color (dark vs grey vs light grey) to create emphasis. Avoid making text too large or too small just for hierarchy.

### 8. Don't use grey text on colored backgrounds

**PRINCIPLE**: Grey text works for de-emphasis on white backgrounds, but on colored backgrounds, use colors that have the same hue as the background.

**THE PROBLEM**: Grey text on colored backgrounds reduces contrast through desaturation, which can make text look dull, washed out, or even disabled.

**THE SOLUTION**: Hand-pick colors based on the background color rather than using grey.

**IMPLEMENTATION**:
```
Instead of using grey (#666666) on colored backgrounds:

Step 1: Start with the background color
Step 2: Adjust saturation and lightness while keeping the same hue
Step 3: Test for sufficient contrast and readability

For a teal background:
- Background: hsl(183, 70%, 64%)
- De-emphasized text: hsl(183, 70%, 84%) [lighter, same hue]
- Not: #999999 [grey, different hue]
```

**VISUAL GUIDELINES**:
- On white backgrounds: use grey for de-emphasis
- On colored backgrounds: use lighter/darker versions of the same hue
- Always maintain sufficient contrast for accessibility
- Hand-pick colors rather than relying on opacity reductions

**REAL EXAMPLE**: On a green card, use a lighter green for secondary text instead of grey. On a blue header, use a lighter blue for subtitles instead of grey.

**LLM INSTRUCTION**: When de-emphasizing text on colored backgrounds, never use grey. Instead, take the background color and adjust its lightness and saturation while keeping the same hue. This maintains visual harmony and prevents washed-out appearance.

### 9. Emphasize by de-emphasizing

**PRINCIPLE**: Sometimes the best way to make an element stand out is to make everything else less prominent rather than making the target element more prominent.

**THE PROBLEM**: When a main element isn't standing out enough, the instinct is to make it bigger, bolder, or more colorful. But sometimes there's nothing more you can add to it.

**THE SOLUTION**: Instead of further emphasizing the element you want to stand out, de-emphasize the elements that are competing with it.

**IMPLEMENTATION**:
```
Instead of making active navigation items more prominent:
- Make inactive navigation items softer/lighter
- Reduce contrast on secondary elements
- Remove background colors from competing elements

For content areas:
- Remove background colors from sidebars
- Use lighter colors for secondary content
- Reduce visual weight of non-essential elements
```

**VISUAL GUIDELINES**:
- Give inactive navigation items softer colors
- Remove background colors that compete with main content
- Use lighter text colors for secondary information
- Reduce visual weight rather than adding more emphasis

**REAL EXAMPLE**: In a navigation bar, instead of making the active item blue and bold, make inactive items light grey and keep the active item normal dark color.

**LLM INSTRUCTION**: When an element needs more prominence, first try reducing the visual weight of surrounding elements. Remove competing colors, reduce contrast, and simplify the visual design of non-essential elements before making the target element more prominent.

### 10. Labels are a last resort

**PRINCIPLE**: Avoid explicit labels when you can communicate the same information through formatting, context, or combined label-value presentations.

**THE PROBLEM**: Using explicit "label: value" format for every piece of data makes it difficult to create hierarchy and often results in repetitive, cluttered interfaces.

**THE SOLUTION**: Use format, context, and smart presentation to communicate information without labels.

**IMPLEMENTATION STRATEGIES**:

**Strategy 1: You might not need a label at all**
- Email format clearly indicates it's an email address
- Phone number format clearly indicates it's a phone number  
- Price format ($19.99) clearly indicates it's a price
- Context often provides sufficient clarity

**Strategy 2: Combine labels and values**
- Instead of "In stock: 12" → "12 left in stock"
- Instead of "Bedrooms: 3" → "3 bedrooms"
- Instead of "Status: Active" → "Active subscription"

**Strategy 3: Labels are secondary**
- When labels are necessary, make them visually secondary
- Use smaller, lighter text for labels
- Put emphasis on the actual data values

**VISUAL GUIDELINES**:
- Rely on formatting and context first
- When labels are needed, integrate them with values
- Make labels visually secondary to the data
- Use typography hierarchy to emphasize values over labels

**REAL EXAMPLE**: In a user profile, instead of "Name: John Smith, Email: john@example.com", use "John Smith" as a heading with "john@example.com" below in smaller text.

**LLM INSTRUCTION**: Before adding explicit labels, ask if the format or context makes the data type obvious. When labels are necessary, integrate them with the values or make them visually secondary. Prioritize the actual data over the labels in your visual hierarchy.

---

## Layout and Spacing

### 11. Start with too much white space

**PRINCIPLE**: Begin designs with generous white space, then remove it until you're happy with the result, rather than adding white space to cramped designs.

**THE PROBLEM**: Most designers add white space incrementally, only giving elements the minimum breathing room needed to not look "actively bad." This results in adequate but not great designs.

**THE SOLUTION**: Start with way too much space, then systematically remove it until the design feels right.

**IMPLEMENTATION**:
```
Instead of: Design tight → Add space until acceptable
Do this: Design with excessive space → Remove space until optimal

Start with:
- 2-3x normal padding
- Large margins between sections  
- Generous line spacing
- Expansive element spacing

Then reduce systematically until it feels balanced
```

**WHEN TO KEEP DENSE LAYOUTS**:
- Dashboards where lots of information must be visible at once
- Data tables that need to show many rows
- Interfaces where screen real estate is at a premium
- Make this a deliberate decision, not the default

**VISUAL GUIDELINES**:
- White space should be removed, not added
- Start generously, then optimize
- Dense layouts should be intentional choices
- What seems "too much" in isolation often feels "just right" in context

**REAL EXAMPLE**: A review widget that seems to have excessive padding when viewed alone will feel appropriately spaced when placed in the context of a complete page.

**LLM INSTRUCTION**: When creating any interface element, start with generous spacing between all elements. Then systematically reduce spacing until the design feels balanced. Dense layouts should only be used when showing maximum information is more important than visual comfort.

---

## Designing Text

### 12. Establish a type scale

**PRINCIPLE**: Create a constrained set of font sizes rather than picking font sizes arbitrarily. This ensures consistency and speeds up design decisions.

**THE PROBLEM**: Without a systematic approach to font sizes, interfaces end up with too many similar sizes (12px, 13px, 14px) that create inconsistency and decision paralysis.

**MODULAR SCALES (NOT RECOMMENDED)**:
While mathematically appealing, modular scales (like 4:5 ratios) have practical problems:
- Often result in fractional pixel values
- May not provide enough size options for interface design
- Can be too limiting for practical UI needs

**HAND-CRAFTED SCALES (RECOMMENDED)**:
For interface design, manually pick values that:
- Avoid fractional pixels
- Provide sufficient variety for your needs
- Align with your spacing and sizing systems

**RECOMMENDED TYPE SCALE**:
```
12px - Small supporting text, captions
14px - Default body text, form inputs  
16px - Larger body text, default web size
18px - Subheadings, emphasized text
20px - Small headings
24px - Medium headings  
30px - Large headings
36px - Display text
48px - Hero text, major displays
60px - Large displays
72px - Extra large displays
```

**IMPLEMENTATION GUIDELINES**:
- Constrained enough to speed up decision-making
- Not so limited that you feel you're missing useful sizes
- Avoid sizes that differ by only 1-2px
- Ensure sufficient contrast between adjacent sizes

**VISUAL GUIDELINES**:
- Fewer, more distinct sizes work better than many similar sizes
- Your scale should align with your spacing system
- Test sizes in actual interface contexts, not in isolation

**LLM INSTRUCTION**: Always define a type scale before designing interfaces. Use the recommended scale above or create a similar hand-crafted scale. Never pick font sizes arbitrarily - always choose from your predefined system. This ensures consistency and prevents decision paralysis.

---

## Working with Color

### 13. Ditch hex for HSL

**PRINCIPLE**: Use HSL (Hue, Saturation, Lightness) instead of hex or RGB for more intuitive color manipulation and better design systems.

**THE PROBLEM**: Hex and RGB colors that visually look similar appear completely different in code, making it hard to create systematic color variations.

**WHY HSL IS BETTER**: HSL represents colors using attributes that match how humans naturally perceive color:
- **Hue**: Color position on the color wheel (0° = red, 120° = green, 240° = blue)
- **Saturation**: How colorful/vivid the color looks (0% = grey, 100% = vibrant)
- **Lightness**: How light or dark the color is (0% = black, 100% = white)

**PRACTICAL BENEFITS**:
```
Creating color variations is intuitive:
Base color: hsl(220, 60%, 50%)  // Blue
Lighter: hsl(220, 60%, 70%)     // Same blue, lighter
Darker: hsl(220, 60%, 30%)      // Same blue, darker
Less saturated: hsl(220, 30%, 50%)  // Same blue, less vivid

With hex, these relationships are invisible:
Base: #3B82F6
Lighter: #93C5FD  // No obvious relationship in code
Darker: #1E40AF   // No obvious relationship in code
```

**IMPLEMENTATION**:
- Define colors in HSL format
- Keep hue consistent within color families
- Vary saturation and lightness for different shades
- Use consistent saturation levels across different hues

**VISUAL GUIDELINES**:
- HSL makes color system creation more systematic
- Easier to create accessible color contrast
- More intuitive for creating hover states and variations
- Better for maintaining color relationships

**LLM INSTRUCTION**: Always use HSL color format instead of hex or RGB. When creating color variations, maintain the same hue and adjust only saturation and lightness. This makes color systems more systematic and easier to maintain.

### 14. You need more colors than you think

**PRINCIPLE**: Create comprehensive color palettes with multiple shades of each color, not just single colors. Most interfaces need 8-10 shades of each color.

**THE PROBLEM**: Simple color palette generators that provide 3-5 "perfect" colors are not sufficient for real interface design. You need many variations for different use cases.

**WHAT YOU ACTUALLY NEED**:
```
For each primary color, create 8-10 shades:
- 50: Very light (backgrounds, subtle highlights)
- 100: Light (hover states, borders)
- 200: Light medium (disabled states)
- 300: Medium light (placeholders)
- 400: Medium (secondary text)
- 500: Base color (primary use)
- 600: Medium dark (hover states)
- 700: Dark (active states)
- 800: Very dark (headings)
- 900: Darkest (high contrast text)
```

**USE CASES FOR DIFFERENT SHADES**:
- **50-200**: Backgrounds, subtle UI elements
- **300-400**: Borders, disabled elements, placeholders
- **500-600**: Primary buttons, links, main brand color
- **700-800**: Text, icons, active states
- **900**: High contrast elements, dark text on light backgrounds

**IMPLEMENTATION**:
```css
/* Blue color system */
--blue-50: hsl(214, 100%, 97%);
--blue-100: hsl(214, 95%, 93%);
--blue-200: hsl(213, 97%, 87%);
--blue-300: hsl(212, 96%, 78%);
--blue-400: hsl(213, 94%, 68%);
--blue-500: hsl(217, 91%, 60%);  /* Base blue */
--blue-600: hsl(221, 83%, 53%);
--blue-700: hsl(224, 76%, 48%);
--blue-800: hsl(226, 71%, 40%);
--blue-900: hsl(224, 64%, 33%);
```

**VISUAL GUIDELINES**:
- Light shades (50-200) for backgrounds and subtle elements
- Medium shades (300-600) for interactive elements
- Dark shades (700-900) for text and high-contrast elements
- Maintain consistent hue across all shades
- Test contrast ratios for accessibility

**REAL EXAMPLE**: A button needs a base color, hover state, active state, disabled state, and possibly a subtle background version - that's already 5 different shades of one color.

**LLM INSTRUCTION**: Never use just one shade of a color. For every color in your palette, create 8-10 shades ranging from very light to very dark. This provides the flexibility needed for real interface design and ensures you have appropriate colors for all use cases.

---

## Visual Implementation Guidelines & Do's/Don'ts

### 15. Label implementation specifics

**DO's:**
- **De-emphasize labels when necessary**: Use smaller font size, lighter font weight, and reduced color contrast
- **Use visual hierarchy for label-value pairs**: Make the value more prominent than the label
- **Combine labels with values naturally**: "12 left in stock" instead of "Stock: 12"
- **Consider context**: Email format (user@domain.com) often doesn't need a "Email:" label

**DON'T's:**
- ❌ **Don't make labels compete with data**: Labels should support, not overshadow the actual information
- ❌ **Don't use equal emphasis**: Avoid making labels and values the same visual weight
- ❌ **Don't over-label obvious data**: Phone numbers, email addresses, and prices are often self-evident

**IMPLEMENTATION:**
```css
/* DO: Proper label hierarchy */
.data-value {
  font-weight: 600;
  color: #1a1a1a;
  font-size: 16px;
}

.data-label {
  font-weight: 400;
  color: #6b7280;
  font-size: 14px;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

/* DON'T: Equal emphasis */
.bad-label, .bad-value {
  font-weight: 500;
  color: #374151;
  font-size: 16px;
}
```

### 16. Separate visual hierarchy from document hierarchy

**DO's:**
- **Use semantic HTML**: Use h1, h2, h3 for SEO and accessibility
- **Style based on visual needs**: Override default heading sizes with CSS
- **Make section titles small**: Often titles should be labels, not dominanting headlines
- **Consider hiding semantic elements**: Use `position: absolute; left: -9999px` for screen readers only

**DON'T's:**
- ❌ **Don't let HTML determine visual size**: h1 doesn't have to be the largest text on screen
- ❌ **Don't make section titles too prominent**: They're often just organizational labels
- ❌ **Don't skip semantic markup**: Always use proper HTML structure regardless of visual appearance

**IMPLEMENTATION:**
```css
/* DO: Semantic markup with visual override */
h1 {
  font-size: 18px;        /* Smaller than default */
  font-weight: 400;       /* Lighter than default */
  color: #6b7280;         /* De-emphasized */
  margin-bottom: 24px;
}

.hero-title {
  font-size: 48px;        /* Actually prominent element */
  font-weight: 700;
  color: #111827;
}

/* Screen reader only heading */
.sr-only {
  position: absolute;
  left: -9999px;
  width: 1px;
  height: 1px;
  overflow: hidden;
}
```

### 17. Balance weight and contrast

**DO's:**
- **Understand surface area impact**: Bold text and solid icons cover more pixels, appearing heavier
- **Use lighter colors for heavy elements**: Reduce contrast of bold text and solid icons
- **Maintain visual balance**: Heavy elements need lighter colors to feel balanced
- **Test icon-text combinations**: Solid icons often overpower adjacent text

**DON'T's:**
- ❌ **Don't use full contrast for bold elements**: Bold text in pure black can overwhelm
- ❌ **Don't ignore icon weight**: Solid icons are visually heavier than outline icons
- ❌ **Don't use equal colors for different weights**: Adjust contrast based on visual weight

**IMPLEMENTATION:**
```css
/* DO: Balanced contrast */
.regular-text {
  color: #111827;          /* Full contrast for normal weight */
  font-weight: 400;
}

.bold-text {
  color: #374151;          /* Reduced contrast for bold weight */
  font-weight: 700;
}

.solid-icon {
  color: #6b7280;          /* Even lighter for heavy icons */
  fill: currentColor;
}

/* DON'T: Same contrast for all weights */
.bad-text, .bad-bold, .bad-icon {
  color: #111827;          /* Too heavy for bold elements */
}
```

### 18. Establish a spacing and sizing system

**DO's:**
- **Start with a base value**: Use 16px as your foundation (default browser font size)
- **Use multiplicative factors**: Create scales with clear relationships (4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px, 96px, 128px, 192px, 256px, 384px, 512px, 640px, 768px)
- **Ensure 25% minimum difference**: No two values should be closer than ~25% apart
- **Apply consistently**: Use your scale for margins, padding, widths, heights
- **Label your scales**: Name them clearly (xs, sm, md, lg, xl, 2xl, etc.)

**DON'T's:**
- ❌ **Don't use linear spacing**: Avoid uniform increments (4px, 8px, 12px, 16px, 20px, 24px)
- ❌ **Don't pick arbitrary values**: Always choose from your predefined scale
- ❌ **Don't make values too close**: Avoid 120px and 125px in the same system
- ❌ **Don't ignore the scale**: Resist the urge to adjust values by 1-2px

**IMPLEMENTATION:**
```css
/* DO: Systematic spacing scale */
:root {
  --space-1: 4px;     /* 0.25rem */
  --space-2: 8px;     /* 0.5rem */
  --space-3: 12px;    /* 0.75rem */
  --space-4: 16px;    /* 1rem */
  --space-6: 24px;    /* 1.5rem */
  --space-8: 32px;    /* 2rem */
  --space-12: 48px;   /* 3rem */
  --space-16: 64px;   /* 4rem */
  --space-24: 96px;   /* 6rem */
  --space-32: 128px;  /* 8rem */
  --space-48: 192px;  /* 12rem */
  --space-64: 256px;  /* 16rem */
  --space-96: 384px;  /* 24rem */
  --space-128: 512px; /* 32rem */
}

.card {
  padding: var(--space-6);        /* 24px */
  margin-bottom: var(--space-8);  /* 32px */
  border-radius: var(--space-2);  /* 8px */
}

/* DON'T: Arbitrary values */
.bad-card {
  padding: 22px;          /* Not in scale */
  margin-bottom: 35px;    /* Not in scale */
  border-radius: 7px;     /* Not in scale */
}
```

### 19. Creating depth with light sources

**DO's:**
- **Establish a consistent light source**: Imagine light coming from above and slightly to the left
- **Raised elements**: Lighter on top, darker on bottom
- **Inset elements**: Darker on top, lighter on bottom
- **Use subtle shadows**: Simulate realistic lighting conditions
- **Layer elements consistently**: Higher elements have more pronounced lighting effects

**DON'T's:**
- ❌ **Don't mix light sources**: Keep lighting direction consistent across interface
- ❌ **Don't ignore physics**: Light effects should follow realistic principles
- ❌ **Don't overdo effects**: Subtle lighting is more believable

**IMPLEMENTATION:**
```css
/* DO: Consistent light source from top-left */
.raised-button {
  background: linear-gradient(145deg, #ffffff, #f0f0f0);
  box-shadow: 
    5px 5px 10px #d1d1d1,
    -5px -5px 10px #ffffff;
}

.inset-field {
  background: linear-gradient(145deg, #e6e6e6, #ffffff);
  box-shadow: 
    inset 5px 5px 10px #d1d1d1,
    inset -5px -5px 10px #ffffff;
}

/* Card appearing raised */
.elevated-card {
  background: #ffffff;
  box-shadow: 
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

/* DON'T: Inconsistent lighting */
.bad-mixed-lighting {
  box-shadow: 
    -5px 5px 10px #d1d1d1,    /* Light from top-right */
    5px -5px 10px #ffffff;    /* Conflicts with above */
}
```

**LLM INSTRUCTION**: When implementing these visual guidelines, always maintain consistency across your interface. Use the provided CSS patterns as starting points, but ensure all measurements come from your spacing scale and all colors come from your color system. Test your implementations by viewing them in context, not in isolation.

---

## Finishing Touches & UX Polish

### 20. Use good photos and images

**DO's:**
- **Invest in quality photography**: Bad photos ruin even the best designs
- **Hire professionals when needed**: For critical imagery, professional photography is worth the investment
- **Use high-resolution images**: Ensure images look crisp on all screen densities
- **Choose images that match your brand**: Images should reinforce your design's personality
- **Consider lighting and composition**: Good photography requires skill in lighting, composition, and color

**DON'T's:**
- ❌ **Don't use low-quality stock photos**: Generic, poorly lit stock photos damage credibility
- ❌ **Don't ignore image optimization**: Large images slow down your interface
- ❌ **Don't use mismatched styles**: Ensure all images have consistent tone and style

**IMPLEMENTATION:**
```css
/* DO: Proper image handling */
.hero-image {
  width: 100%;
  height: 400px;
  object-fit: cover;
  object-position: center;
  border-radius: 8px;
}

/* Responsive images */
.responsive-image {
  max-width: 100%;
  height: auto;
  display: block;
}

/* Loading states for images */
.image-placeholder {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
}

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

### 21. Supercharge the defaults

**DO's:**
- **Replace generic bullets with icons**: Use checkmarks, arrows, or relevant icons
- **Add meaningful icons to features**: Choose icons that relate to the specific content
- **Use branded elements**: Apply your color system and design language to standard elements
- **Make lists more scannable**: Icons help users quickly parse information

**DON'T's:**
- ❌ **Don't use generic bullet points**: Simple dots add no visual interest or meaning
- ❌ **Don't use irrelevant icons**: Icons should enhance understanding, not just decoration
- ❌ **Don't overdo it**: Not every list needs icons, use them purposefully

**IMPLEMENTATION:**
```html
<!-- DON'T: Generic bullets -->
<ul>
  <li>Create your own online presence</li>
  <li>Manage all of your products and inventory</li>
  <li>Effortlessly complete order fulfillment</li>
</ul>

<!-- DO: Meaningful icons -->
<ul class="feature-list">
  <li class="feature-item">
    <svg class="feature-icon check-icon"><!-- checkmark SVG --></svg>
    Create your own online presence
  </li>
  <li class="feature-item">
    <svg class="feature-icon check-icon"><!-- checkmark SVG --></svg>
    Manage all of your products and inventory
  </li>
  <li class="feature-item">
    <svg class="feature-icon check-icon"><!-- checkmark SVG --></svg>
    Effortlessly complete order fulfillment
  </li>
</ul>
```

```css
.feature-list {
  list-style: none;
  padding: 0;
}

.feature-item {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.feature-icon {
  width: 20px;
  height: 20px;
  margin-right: 12px;
  flex-shrink: 0;
}

.check-icon {
  color: #10b981;  /* Green checkmark */
  fill: currentColor;
}

.security-icon {
  color: #3b82f6;  /* Blue for security features */
  fill: currentColor;
}
```

### 22. Don't overlook empty states

**DO's:**
- **Design empty states deliberately**: Don't just handle the happy path with data
- **Provide helpful guidance**: Tell users what they can do to populate the interface
- **Make empty states visually appealing**: Use illustrations, icons, or helpful messaging
- **Consider different empty scenarios**: No data yet, no results found, error states
- **Include clear actions**: Give users obvious next steps

**DON'T's:**
- ❌ **Don't show blank screens**: Never leave users with empty, confusing interfaces
- ❌ **Don't ignore edge cases**: Empty states are common, especially for new users
- ❌ **Don't use generic messages**: Tailor empty state content to the specific context

**IMPLEMENTATION:**
```html
<!-- DO: Helpful empty state -->
<div class="empty-state">
  <div class="empty-state-icon">
    <svg><!-- relevant icon --></svg>
  </div>
  <h3 class="empty-state-title">No contacts yet</h3>
  <p class="empty-state-description">
    Get started by adding your first contact. You can import from your phone 
    or add them manually.
  </p>
  <div class="empty-state-actions">
    <button class="btn btn-primary">Add Contact</button>
    <button class="btn btn-secondary">Import Contacts</button>
  </div>
</div>

<!-- DON'T: Just show loading or nothing -->
<div class="contacts-list">
  <!-- Nothing here when empty -->
</div>
```

```css
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 64px 32px;
  text-align: center;
  min-height: 400px;
}

.empty-state-icon {
  width: 64px;
  height: 64px;
  margin-bottom: 24px;
  color: #9ca3af;
}

.empty-state-title {
  font-size: 20px;
  font-weight: 600;
  color: #111827;
  margin-bottom: 8px;
}

.empty-state-description {
  font-size: 16px;
  color: #6b7280;
  margin-bottom: 32px;
  max-width: 400px;
  line-height: 1.5;
}

.empty-state-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  justify-content: center;
}
```

### 23. Loading and interaction states

**DO's:**
- **Show loading states**: Indicate when actions are processing
- **Disable buttons during actions**: Prevent double-submissions
- **Provide feedback for all interactions**: Hover, focus, active states
- **Use skeleton screens**: Show content structure while loading
- **Give progress indicators**: For longer operations, show progress

**DON'T's:**
- ❌ **Don't leave users guessing**: Always indicate when something is happening
- ❌ **Don't ignore hover states**: Every interactive element needs hover feedback
- ❌ **Don't use generic spinners**: Context-specific loading states are better

**IMPLEMENTATION:**
```css
/* Button states */
.btn {
  position: relative;
  transition: all 0.2s ease;
  border: none;
  cursor: pointer;
}

.btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.btn:active {
  transform: translateY(0);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.12);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.btn.loading {
  color: transparent;
}

.btn.loading::after {
  content: '';
  position: absolute;
  width: 16px;
  height: 16px;
  top: 50%;
  left: 50%;
  margin-left: -8px;
  margin-top: -8px;
  border: 2px solid currentColor;
  border-radius: 50%;
  border-right-color: transparent;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Skeleton loading */
.skeleton {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 4px;
}

.skeleton-text {
  height: 16px;
  margin-bottom: 8px;
}

.skeleton-title {
  height: 24px;
  width: 60%;
  margin-bottom: 16px;
}
```

**LLM INSTRUCTION**: When implementing interfaces, always consider the complete user journey including empty states, loading states, and error states. Every interactive element must have clear visual feedback, and users should never be left wondering if their action was registered. Use the provided CSS patterns to create polished, professional interactions that guide users through your interface confidently.

---

## Summary for LLM Implementation

This guide provides comprehensive UI design principles with specific implementation details for creating professional, user-friendly interfaces. Key areas covered:

1. **Strategic Approach**: Start with features, not layouts; limit choices through design systems
2. **Visual Hierarchy**: Use size, weight, color, and spacing to guide user attention
3. **Typography & Color**: Systematic scales and HSL-based color systems for consistency
4. **Spacing & Layout**: Mathematical spacing systems and proper white space usage
5. **Implementation Details**: Concrete CSS examples, do's/don'ts, and UX considerations

When implementing these principles, always:
- Define your design systems before starting
- Use systematic approaches rather than arbitrary decisions
- Consider all interface states (loading, empty, error)
- Test in context, not isolation
- Maintain consistency across all elements

Each principle includes specific implementation code and clear guidelines for creating interfaces that feel professional, cohesive, and user-friendly.

---
