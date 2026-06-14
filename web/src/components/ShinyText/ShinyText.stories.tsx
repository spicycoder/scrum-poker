import type { Meta, StoryObj } from '@storybook/react';
import ShinyText from './ShinyText';

const meta: Meta<typeof ShinyText> = {
  title: 'Components/ShinyText',
  component: ShinyText,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <div className="p-8 text-center">
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof ShinyText>;

export const Default: Story = {
  args: {
    text: "♠️ Scrum Poker",
  },
};

export const Disabled: Story = {
  args: {
    text: 'Scrum Poker',
    disabled: true,
  },
};

export const YoyoEffect: Story = {
  args: {
    text: 'Back and Forth',
    yoyo: true,
  },
};

export const CustomColors: Story = {
  args: {
    text: 'Custom Colors',
    color: '#7c3aed',
    shineColor: '#a78bfa',
    speed: 3,
  },
};
