import type { Meta, StoryObj } from '@storybook/react';
import Shuffle from './Shuffle';

const meta: Meta<typeof Shuffle> = {
  title: 'Components/Shuffle',
  component: Shuffle,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <div className="p-8">
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof Shuffle>;

export const Default: Story = {
  args: {
    text: "♠️ Scrum Poker",
    tag: 'h2',
  },
};

export const ScrambleEffect: Story = {
  args: {
    text: 'Scramble!',
    scrambleCharset: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    tag: 'h2',
  },
};

export const ColorTransition: Story = {
  args: {
    text: 'Color Shift',
    colorFrom: '#7c3aed',
    colorTo: '#ec4899',
    tag: 'h2',
  },
};

export const RandomStagger: Story = {
  args: {
    text: 'Random Order',
    animationMode: 'random',
    maxDelay: 0.3,
    tag: 'h2',
  },
};
