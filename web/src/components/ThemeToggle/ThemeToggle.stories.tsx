import type { Meta, StoryObj } from '@storybook/react';
import { ThemeProvider } from '../../contexts/ThemeContext';
import { TooltipProvider } from '../ui/tooltip';
import ThemeToggle from './ThemeToggle';

const meta: Meta<typeof ThemeToggle> = {
  title: 'Components/ThemeToggle',
  component: ThemeToggle,
  tags: ['!autodocs'],
  decorators: [
    (Story) => (
      <ThemeProvider>
        <TooltipProvider>
          <Story />
        </TooltipProvider>
      </ThemeProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof ThemeToggle>;

export const Default: Story = {};
