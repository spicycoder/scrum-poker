import type { Meta, StoryObj } from '@storybook/react';
import DotField from './DotField';

const meta: Meta<typeof DotField> = {
  title: 'Core/DotField',
  component: DotField,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <div className="h-[400px] w-full rounded-lg overflow-hidden relative">
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof DotField>;

export const Default: Story = {
  args: {},
};

export const SparseDots: Story = {
  args: {
    dotRadius: 3,
    dotSpacing: 28,
  },
};

export const SparseSparkle: Story = {
  args: {
    dotRadius: 3,
    dotSpacing: 28,
    sparkle: true,
  },
};
